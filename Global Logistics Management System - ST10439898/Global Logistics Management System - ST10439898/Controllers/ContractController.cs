using Global_Logistics_Management_System___ST10439898.Data;
using Global_Logistics_Management_System___ST10439898.Models;
using Global_Logistics_Management_System___ST10439898.Services.Behavioural;
using Global_Logistics_Management_System___ST10439898.Services.Creational;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Global_Logistics_Management_System___ST10439898.Controllers
{
    public class ContractController : Controller
    {
        private readonly GLMSContext _context;
        private readonly IContractFactory _contractFactory;
        private readonly IWebHostEnvironment _environment;

        public ContractController(GLMSContext context, IContractFactory contractFactory, IWebHostEnvironment environment)
        {
            _context = context;
            _contractFactory = contractFactory;
            _environment = environment;
        }

        // GET: Contract
        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, string status)
        {
            // Store filter values in ViewBag to persist in the form
            ViewBag.StartDateFrom = startDateFrom?.ToString("yyyy-MM-dd");
            ViewBag.StartDateTo = startDateTo?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            //fetches the contracts
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            //search functionality
            if (startDateFrom.HasValue)
            {
                contracts = contracts.Where(c => c.startDate >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                contracts = contracts.Where(c => c.startDate <= startDateTo.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                //string to enum
                if (Enum.TryParse<Models.Contract.Status>(status, out var statusEnum))
                {
                    contracts = contracts.Where(c => c.contractStatus == statusEnum);
                }
            }

            //results of search
            var result = await contracts.ToListAsync();

            return View(result);
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.contractID == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contract/Create
        public IActionResult Create()
        {
            ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName");
            return View();
        }

        // POST: Contract/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int clientID, string serviceLevel, IFormFile pdfUpload)
        {
            //fetches the client ID from the database - no client = no contract
            var client = await _context.Clients.FindAsync(clientID);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            //calls factory method to create new contract - automatically sets start and end date and sets contract status as draft
            Models.Contract newContract = _contractFactory.CreateContract(client);

            newContract.serviceLevel = serviceLevel;

            //file upload
            if (pdfUpload != null && pdfUpload.Length > 0)
            {
                //ensures that file is a pdf and not some other file type
                if (Path.GetExtension(pdfUpload.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("", "Action Denied: Only PDF files are allowed for contracts.");
                    ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName", clientID);
                    return View();
                }

                //creates directory for file upload
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                //GUID creates unique file name for upload
                string uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //file path sent to folder
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfUpload.CopyToAsync(fileStream);
                }

                //URL saved
                newContract.signedAgreementPath = "/uploads/" + uniqueFileName;
            }
            else
            {
                ModelState.AddModelError("", "A signed PDF agreement is required.");
                ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName", clientID);
                return View();
            }

            newContract.LastModified = DateTime.Now;

            //completed contract saved to database
            _context.Contracts.Add(newContract);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }





        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName", contract.clientID);
            return View(contract);
        }

        // POST: Contract/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("contractID,clientID,startDate,endDate,contractStatus,serviceLevel")] Models.Contract contract, IFormFile pdfUpload, string existingFilePath)
        {
            if (id != contract.contractID)
            {
                return NotFound();
            }

            //these navigation properties intefere with the saving of the edited contract - removed because the database doesnt lose anything
            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");
            ModelState.Remove("pdfUpload");
            ModelState.Remove("signedAgreementPath");

            //logging/debugging
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName", contract.clientID);
                return View(contract);
            }

            try
            {
                //fetch existing contract
                var existingContract = await _context.Contracts.FindAsync(id);
                if (existingContract == null)
                {
                    return NotFound();
                }

                //update the properties
                existingContract.clientID = contract.clientID;
                existingContract.startDate = contract.startDate;
                existingContract.endDate = contract.endDate;
                existingContract.contractStatus = contract.contractStatus;
                existingContract.serviceLevel = contract.serviceLevel;
                existingContract.LastModified = DateTime.Now;

                //file upload
                if (pdfUpload != null && pdfUpload.Length > 0)
                {
                    if (Path.GetExtension(pdfUpload.FileName).ToLower() != ".pdf")
                    {
                        ModelState.AddModelError("", "Only PDF files are allowed.");
                        ViewData["clientID"] = new SelectList(_context.Clients, "clientID", "clientName", contract.clientID);
                        return View(contract);
                    }

                    //delete the old file
                    if (!string.IsNullOrEmpty(existingContract.signedAgreementPath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, existingContract.signedAgreementPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    //save new contract
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + ".pdf";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfUpload.CopyToAsync(fileStream);
                    }

                    existingContract.signedAgreementPath = "/uploads/" + uniqueFileName;
                }

                _context.Update(existingContract);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(contract.contractID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Contract/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.contractID == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.contractID == id);
        }
    }
}
