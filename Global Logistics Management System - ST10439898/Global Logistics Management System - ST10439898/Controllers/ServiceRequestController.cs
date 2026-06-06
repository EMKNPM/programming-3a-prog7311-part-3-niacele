using Global_Logistics_Management_System___ST10439898.Data;
using Global_Logistics_Management_System___ST10439898.Models;
using Global_Logistics_Management_System___ST10439898.Services.Behavioural;
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
        private readonly GLMSContext _context;
        private readonly ServiceRequestManager _manager;

        public ServiceRequestController(GLMSContext context, ServiceRequestManager manager)
        {
            _context = context;
            _manager = manager;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var gLMSContext = _context.ServiceRequests.Include(s => s.Contract).ThenInclude(c => c.Client);
            return View(await gLMSContext.ToListAsync());
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.requestID == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        public IActionResult Create()
        {
            //only shows active contracts - prevents service requests from being created for contracts that arent active
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.contractStatus == Contract.Status.Active)
                .Select(c => new {
                    c.contractID,
                    DisplayText = $"Contract #{c.contractID} - {c.Client.companyName} ({c.contractStatus})"
                })
                .ToList();

            ViewData["contractID"] = new SelectList(contracts, "contractID", "DisplayText");
            return View();
        }

        // POST: ServiceRequest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("contractID, Description, OriginalCost")] ServiceRequest serviceRequest)
        {
            //fetch parent contract from database
            var parentContract = await _context.Contracts.FindAsync(serviceRequest.contractID);

            if (parentContract == null)
            {
                return NotFound("The specified parent contract could not be found.");
            }

            parentContract.SyncStateFromStatus();

            try
            {
                //checks state -> triggers currency conversion
                await _manager.CreateServiceRequestAsync(parentContract, serviceRequest);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                var contracts = _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.contractStatus == Contract.Status.Active)
                    .Select(c => new {
                        c.contractID,
                        DisplayText = $"Contract #{c.contractID} - {c.Client.clientName} ({c.contractStatus})"
                    })
                    .ToList();

                ViewData["contractID"] = new SelectList(contracts, "contractID", "DisplayText", serviceRequest.contractID);
                return View(serviceRequest);
            }
        }

        // GET: ServiceRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            var contracts = _context.Contracts
                .Include(c => c.Client)
                .Select(c => new {
                    c.contractID,
                    DisplayText = $"Contract #{c.contractID} - {c.Client.clientName} ({c.contractStatus})"
                })
                .ToList();

            ViewData["contractID"] = new SelectList(contracts, "contractID", "DisplayText", serviceRequest.contractID);

            ViewBag.OriginalCost = serviceRequest.OriginalCost;

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("requestID,contractID,Description,requestStatus")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.requestID)
            {
                return NotFound();
            }

            //fields arent being bound so they are removed
            ModelState.Remove("Contract");
            ModelState.Remove("OriginalCost");
            ModelState.Remove("CostinZAR");

            if (ModelState.IsValid)
            {
                try
                {
                    //retrieve existing requests
                    var existingRequest = await _context.ServiceRequests.FindAsync(id);
                    if (existingRequest == null)
                    {
                        return NotFound();
                    }

                    //update fields
                    existingRequest.contractID = serviceRequest.contractID;
                    existingRequest.Description = serviceRequest.Description;
                    existingRequest.requestStatus = serviceRequest.requestStatus;

                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.requestID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var contracts = _context.Contracts
                .Include(c => c.Client)
                .Select(c => new {
                    c.contractID,
                    DisplayText = $"Contract #{c.contractID} - {c.Client.clientName} ({c.contractStatus})"
                })
                .ToList();

            ViewData["contractID"] = new SelectList(contracts, "contractID", "DisplayText", serviceRequest.contractID);
            return View(serviceRequest);
        }
        private async Task<decimal> ConvertCurrencyAmount(decimal amount, string fromCurrency)
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
                        return amount * zarRate;
                    }
                }

                return amount;
            }
            catch
            {
                return amount; 
            }
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.requestID == id);
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
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.requestID == id);
        }
    }
}
