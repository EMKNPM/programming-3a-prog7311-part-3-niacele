using Global_Logistics_Management_System___ST10439898.ViewModels;
using Global_Logistics_Management_System___ST10439898.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Global_Logistics_Management_System___ST10439898.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestApiService _serviceRequestApiService;
        private readonly ContractApiService _contractApiService;

        public ServiceRequestController(ServiceRequestApiService serviceRequestApiService, ContractApiService contractApiService)
        {
            _serviceRequestApiService = serviceRequestApiService;
            _contractApiService = contractApiService;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var requests = await _serviceRequestApiService.GetServiceRequestsAsync();
            return View(requests);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _serviceRequestApiService.GetServiceRequestByIdAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        public async Task<IActionResult> Create()
        {
            // fetch only the contracts with active status to populate drop down
            var contracts = await _contractApiService.GetContractsAsync(null, null, "Active");

            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();

            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "DisplayText");
            return View(new ServiceRequestViewModel());
        }

        // POST: ServiceRequest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel serviceRequest)
        {
            if (!ModelState.IsValid)
            {
                await PopulateContractsDropdownAsync(serviceRequest.contractID);
                return View(serviceRequest);
            }


            var response = await _serviceRequestApiService.CreateServiceRequestAsync(serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            await PopulateContractsDropdownAsync(serviceRequest.contractID);
            return View(serviceRequest);
        }

        // GET: ServiceRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _serviceRequestApiService.GetServiceRequestByIdAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            await PopulateAllContractsDropdownAsync(serviceRequest.contractID);
            ViewBag.originalCost = serviceRequest.originalCost;
            return View(serviceRequest);
        }

        // POST: ServiceRequest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequestViewModel serviceRequest)
        {
            if (id != serviceRequest.requestID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateAllContractsDropdownAsync(serviceRequest.contractID);
                return View(serviceRequest);
            }

            var response = await _serviceRequestApiService.UpdateServiceRequestAsync(id, serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            // display error message if this fails
            ModelState.AddModelError("", "API Error: Failed to save modifications.");
            await PopulateAllContractsDropdownAsync(serviceRequest.contractID);
            return View(serviceRequest);
        }

        // this is for the currency conversion and makes it so that the user wont have to press calculate in order to see the converted price
        [HttpGet]
        public async Task<IActionResult> ConvertCurrency(decimal amount, string fromCurrency)
        {
            var result = await _serviceRequestApiService.ConvertCurrencyAsync(amount, fromCurrency);
            if (result != null && result.Success)
            {
                return Json(new { success = true, zarAmount = result.ZarAmount });
            }
            return Json(new { success = false, message = "Dynamic currency computation failed via backend." });
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _serviceRequestApiService.GetServiceRequestByIdAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            return View(serviceRequest);
        }

        // POST: ServiceRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _serviceRequestApiService.DeleteServiceRequestAsync(id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Delete), new { id = id, error = "Delete request rejected." });
        }

        // helper methods that populate the dropdown for the contract during creation and editing a service request
        private async Task PopulateContractsDropdownAsync(int selectedId)
        {
            var contracts = await _contractApiService.GetContractsAsync(null, null, "Active");
            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();
            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "DisplayText", selectedId);
        }

        private async Task PopulateAllContractsDropdownAsync(int selectedId)
        {
            var contracts = await _contractApiService.GetContractsAsync(null, null, null);
            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();
            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "DisplayText", selectedId);
        }
    }
}