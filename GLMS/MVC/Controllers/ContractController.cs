using Global_Logistics_Management_System___ST10439898.Services;
using Global_Logistics_Management_System___ST10439898.ViewModels;
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
        private readonly ContractApiService _contractApiService;
        private readonly ClientApiService _clientApiService;

        public ContractController(ContractApiService contractApiService, ClientApiService  clientApiService)
        {
            _contractApiService = contractApiService;
            _clientApiService = clientApiService;
        }

        // GET: Contract
        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, string status)
        {
            //converts datetime into string so the API can digest it
            string ? fromStr = startDateFrom?.ToString("yyyy-MM-dd");
            string? toStr = startDateTo?.ToString("yyyy-MM-dd");

            ViewBag.StartDateFrom = fromStr;
            ViewBag.StartDateTo = toStr;
            ViewBag.Status = status;

            //fetch data from API 
            var contracts = await _contractApiService.GetContractsAsync(fromStr, toStr, status);

            return View(contracts);
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Changed from DeleteContractAsync to GetContractByIdAsync
            var contract = await _contractApiService.GetContractByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contract/Create
        public async Task<IActionResult> Create()
        {
            //pull clients to populate drop down
            var clients = await _clientApiService.GetClientsAsync();
            ViewData["clientID"] = new SelectList(clients, "clientID", "clientName");
            return View(new ContractViewModel());
        }

        // POST: Contract/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractViewModel contract)
        {
            //verify that the contract is a pdf
            if (contract.pdfUpload == null || contract.pdfUpload.Length == 0)
            {
                ModelState.AddModelError("", "A signed PDF agreement is required.");
            }
            else if (Path.GetExtension(contract.pdfUpload.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("", "Action Denied: Only PDF files are allowed for contracts.");
            }

            if (!ModelState.IsValid)
            {
                var clients = await _clientApiService.GetClientsAsync();
                ViewData["clientID"] = new SelectList(clients, "clientID", "clientName", contract.clientID);
                return View(contract);
            }

            var response = await _contractApiService.CreateContractAsync(contract);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "API Error: The backend data repository rejected the contract creation.");
            var finalClients = await _clientApiService.GetClientsAsync();
            ViewData["clientID"] = new SelectList(finalClients, "clientID", "clientName", contract.clientID);
            return View(contract);
        }
         
        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _contractApiService.GetContractByIdAsync(id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            var clients = await _clientApiService.GetClientsAsync();
            ViewData["clientID"] = new SelectList(clients, "clientID", "clientName", contract.clientID);
            return View(contract);
        }

        // POST: Contract/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractViewModel contract)
        {
            if (id != contract.contractID)
            {
                return NotFound();
            }

            if (contract.pdfUpload != null && Path.GetExtension(contract.pdfUpload.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("", "Only PDF files are allowed.");
            }

            if (!ModelState.IsValid)
            {
                var clients = await _clientApiService.GetClientsAsync();
                ViewData["clientID"] = new SelectList(clients, "clientID", "clientName", contract.clientID);
                return View(contract);
            }

            var response = await _contractApiService.UpdateContractAsync(id, contract);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "API Error: Failed to commit contract revisions to the database.");
            var finalClients = await _clientApiService.GetClientsAsync();
            ViewData["clientID"] = new SelectList(finalClients, "clientID", "clientName", contract.clientID);
            return View(contract);
        }

        // GET: Contract/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractApiService.GetContractByIdAsync(id);
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
            var response = await _contractApiService.DeleteContractAsync(id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Delete), new { id = id, error = "Deletion command rejected by backend." });
        }

    }
}
