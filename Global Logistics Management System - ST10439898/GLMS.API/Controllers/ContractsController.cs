using GLMS.API.Data;
using GLMS.API.Services.Creational;
using GLMS.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        //database context + design pattern
        private readonly GLMSContext _context;
        private readonly IContractFactory _contractFactory;

        //creates physical folder location for contracts
        private readonly string _uploadsFolder = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
    
        //constructor for dependency injection
        public ContractsController (GLMSContext context, IContractFactory contractFactory)
        {
            _context = context;
            _contractFactory = contractFactory;

            //checks that contracts folder actually exists - if not, create it 
            if(!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        //GET - all contracts
        [HttpGet]
        public async Task<IActionResult> GetContracts([FromQuery] string? startDateFrom, [FromQuery] string? startDateTo, [FromQuery] string? status) //[FromQuery] maps URL prameters to method arguements + allows for easy filtering by API
        {
            var contractQuery = _context.Contracts.Include(c => c.Client).AsQueryable(); //AsQueryable() instead of toList() - prepares server for the query rather than pulling all the contracts NOW

            if(!string.IsNullOrEmpty(startDateFrom) && DateTime.TryParse(startDateFrom, out var dateFrom))
            {
                contractQuery = contractQuery.Where(c => c.startDate >= dateFrom);
            }

            if (!string.IsNullOrEmpty(startDateTo) && DateTime.TryParse(startDateFrom, out var dateTo))
            {
                contractQuery = contractQuery.Where(c => c.startDate <= dateTo);
            }

            if (!string.IsNullOrEmpty(status))
            {
                //converts the JSON string parameter into the Enum data type
                if (Enum.TryParse<Contract.Status>(status, true, out var statusEnum))
                {
                    contractQuery = contractQuery.Where(c => c.contractStatus == statusEnum);
                }
            }

            //now the LINQ query gets executed
            var contracts = await contractQuery.ToListAsync();

            //returns JSON data
            return Ok(contracts);


        }

        //GET - specific contracts by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(int id)
        {
            //query to search database for contracts with ID
            var contractQuery = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.contractID == id);

            //return error message if ID doesnt exist
            if (contractQuery == null)
            {
                return NotFound(new { message = $"Contract with ID {id} was not found." });
            }

            //ensures that contract state isn't left empty
            contractQuery.SyncStateFromStatus();

            //returns JSON data
            return Ok(contractQuery); 
        }
        //POST - new contract
        [HttpPost]
        public async Task<IActionResult> CreateContract([FromForm] int clientID, [FromForm] string serviceLevel, IFormFile pdfUpload)
        {
            //for new ontract to be made, the client that the contract belongs to should exist
            var client = await _context.Clients.FindAsync(clientID);
            if (client == null)
            {
                //return error message if client doesnt exist
                return BadRequest(new { message = "Action Denied: Targeted Client does not exist." });
            }

            //all contracts need to come with a signed contract
            if (pdfUpload == null || pdfUpload.Length == 0)
            {
                //return error message if contract is missing
                return BadRequest(new { message = "Validation Error: A signed PDF agreement file is mandatory." });
            }

            //only allows for pdfs and nothing else
            if (Path.GetExtension(pdfUpload.FileName).ToLower() != ".pdf")
            {
                return BadRequest(new { message = "Security Exception: Upload rejected. Only .pdf file structures are permitted." });
            }

            //new contract object
            Contract newContract = _contractFactory.CreateContract(client);
            newContract.serviceLevel = serviceLevel;

            //unique file name
            string uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
            string filePath = Path.Combine(_uploadsFolder, uniqueFileName);

            //saved to shared environment directory path
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pdfUpload.CopyToAsync(fileStream);
            }

            //sets path location for database context
            newContract.signedAgreementPath = "/uploads/" + uniqueFileName;
            newContract.LastModified = DateTime.Now;

            //saves new contract + changes made to database
            _context.Contracts.Add(newContract);
            await _context.SaveChangesAsync();

            //returns special success message
            return CreatedAtAction(nameof(GetContractById), new { id = newContract.contractID }, newContract);
        }
    }
}
