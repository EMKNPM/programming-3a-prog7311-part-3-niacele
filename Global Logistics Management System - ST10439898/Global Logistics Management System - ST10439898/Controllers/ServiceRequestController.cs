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
        private readonly ServiceRequestApiService _servicerRequestApiService;
        private readonly ContractApiService _contractApService;

        public ServiceRequestController(ServiceRequestApiService serviceRequestApiService, ContractApiService contractApiService)
        {
            _servicerRequestApiService = serviceRequestApiService;
            _contractApService = contractApiService;
        }
        

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var requests = await _servicerRequestApiService.GetServiceRequestsAsync();
            return View(requests);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var serviceRequest = await _servicerRequestApiService.GetServiceRequestByIdAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        public IActionResult Create()
        {
            //fetch only the contracts with active status to populate drop down
            var contracts = await _servicerRequestApiService.GetContractsAsync(null, null, "Active");

            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();

            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "displayText");
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

            var response = await _servicerRequestApiService.CreateServiceRequestAsync(serviceRequest);

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
            var serviceRequest = await _servicerRequestApiService.GetServiceRequestByIdAsync(id);
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
        public async Task<IActionResult> Edit(int id,  ServiceRequestViewModel serviceRequest)
        {

            if (id != serviceRequest.RequestID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateAllContractsDropdownAsync(serviceRequest.contractID);
                return View(serviceRequest);
            }

            var response = await _servicerRequestApiService.UpdateServiceRequestAsync(id, serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            //display error message if this fails
            ModelState.AddModelError("", "API Error: Failed to save modifications.");
            await PopulateAllContractsDropdownAsync(serviceRequest.contractID);
            return View(serviceRequest);
        }

        //this is for the currency conversion and makes it so that the user wont have to press calculate in order to see the converted price
        [HttpGet]
        public async Task<IActionResult> ConvertCurrency(decimal amount, string fromCurrency)
        {
            var result = await _servicerRequestApiService.ConvertCurrencyAsync(amount, fromCurrency);
            if (result != null && result.Success)
            {
                return Json(new { success = true, zarAmount = result.ZarAmount });
            }
            return Json(new { success = false, message = "Dynamic currency computation failed via backend." });
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var serviceRequest = await _servicerRequestApiService.GetServiceRequestByIdAsync(id);
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
            var response = await _requestApiService.DeleteServiceRequestAsync(id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Delete), new { id = id, error = "Delete request rejected." });
        }

        // GET: ServiceRequest/ConvertCurrency
        [HttpGet]
        public async Task<IActionResult> ConvertCurrency(decimal amount, string fromCurrency)
        {
            try
            {
                string apiURL = $"https://v6.exchangerate-api.com/v6/ea39e0973f82c7a1cf494324/latest/{fromCurrency}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiURL);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    if (root.GetProperty("result").GetString() == "success")
                    {
                        JsonElement rates = root.GetProperty("conversion_rates");
                        decimal zarRate = rates.GetProperty("ZAR").GetDecimal();
                        decimal convertedAmount = amount * zarRate;

                        return Json(new { success = true, zarAmount = convertedAmount });
                    }
                }

                return Json(new { success = false, message = "Conversion failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //helper methods that pupulate the dropdown for the contract during creation and editing a service request
        private async Task PopulateContractsDropdownAsync(int selectedId)
        {
            var contracts = await _contractApiService.GetContractsAsync(null, null, "Active");
            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();
            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "displayText", selectedId);
        }

        private async Task PopulateAllContractsDropdownAsync(int selectedId)
        {
            var contracts = await _contractApiService.GetContractsAsync(null, null, null);
            var dropdownData = contracts.Select(c => new {
                c.contractID,
                DisplayText = $"Contract #{c.contractID} - {c.clientCompanyName} ({c.contractStatus})"
            }).ToList();
            ViewData["contractID"] = new SelectList(dropdownData, "contractID", "dsplayText", selectedId);
        }

    }
}
