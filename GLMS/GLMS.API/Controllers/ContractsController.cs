using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Services.Creational;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        // PUT - update existing contract
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromForm] int contractID, [FromForm] int clientID, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string contractStatus, [FromForm] string serviceLevel, [FromForm] string signedAgreementPath, IFormFile? pdfUpload)
        {
            try
            {
                // ensures URL ID matches form contractID
                if (id != contractID)
                {
                    return BadRequest(new { message = "Update rejected: URL ID parameter does not match form data ID" });
                }

                // find existing contract
                var existingContract = await _context.Contracts
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.contractID == id);

                if (existingContract == null)
                {
                    return NotFound(new { message = $"Update failed: Contract with ID {id} does not exist." });
                }

                // update contract properties
                existingContract.clientID = clientID;
                existingContract.serviceLevel = serviceLevel;

                // Handle both integer and string status values
                if (int.TryParse(contractStatus, out int statusInt))
                {
                    existingContract.contractStatus = (Contract.Status)statusInt;
                }
                else
                {
                    existingContract.contractStatus = Enum.Parse<Contract.Status>(contractStatus);
                }

                existingContract.startDate = DateTime.Parse(startDate);
                existingContract.endDate = DateTime.Parse(endDate);

                // FIX: Only update the signedAgreementPath if a new file is uploaded
                // Otherwise, keep the existing path from the database
                if (pdfUpload != null && pdfUpload.Length > 0)
                {
                    // New PDF uploaded - handle it
                    if (Path.GetExtension(pdfUpload.FileName).ToLower() != ".pdf")
                    {
                        return BadRequest(new { message = "Only PDF files are allowed." });
                    }

                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(existingContract.signedAgreementPath))
                    {
                        string oldFilePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", existingContract.signedAgreementPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
                    string filePath = Path.Combine(_uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfUpload.CopyToAsync(fileStream);
                    }

                    existingContract.signedAgreementPath = "/uploads/" + uniqueFileName;
                }
                // FIX: DO NOT update signedAgreementPath if no new PDF is uploaded
                // Keep the existing value from the database

                existingContract.LastModified = DateTime.Now;

                // sync state from status (for the state pattern)
                existingContract.SyncStateFromStatus();

                _context.Entry(existingContract).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Ok(existingContract);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, new { message = "Update failed", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // DELETE - delete a specific contract
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            // search database for contract with matching ID
            var contract = await _context.Contracts
                .Include(c => c.ServiceRequests)  // include related service requests
                .FirstOrDefaultAsync(c => c.contractID == id);

            if (contract == null)
            {
                // return error message if contract with this ID doesn't exist
                return NotFound(new { message = $"Delete operation failed: Contract with ID {id} was not found." });
            }

            // check for service requests linked to this contract
            if (contract.ServiceRequests != null && contract.ServiceRequests.Any())
            {
                // cant delete contract if there are active service requests
                return BadRequest(new { message = $"Cannot delete Contract {id} because it has {contract.ServiceRequests.Count} associated service requests. Delete those first." });

            }

            // delete contract from database
            _context.Contracts.Remove(contract);

            // save changes made to database
            await _context.SaveChangesAsync();

            // return JSON data
            return Ok(new { message = $"Contract {id} successfully removed from system records." });
        }


    }
}
