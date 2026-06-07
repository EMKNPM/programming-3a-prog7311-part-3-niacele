using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Global_Logistics_Management_System___ST10439898.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace Global_Logistics_Management_System___ST10439898.Controllers
{
    public class ClientController : Controller
    {
        private readonly ClientApiService _apiService;

        public ClientController( ClientApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            //pulls client list through API service
            var clients = await _apiService.GetClientAsync();

            return ViewModels(clients);
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _apiService.GetClientByIdAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client) //no more Bind because ViewModel validates the incoming data
        {
            if(!ModelState.IsValid)
    {
                //logging
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(client);
            }

            //create client through API service
            var response = await _apiService.CreateClientAsync(client);

            if(response.IsSuccessStatusCode)
            {
                //returns this only if client creation was successful
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "API Error: Client account profile could not be saved.");
            return ViewModels(client);
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _apiService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Client/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client) //no more Bind because ViewModel validates incoming data
        {
            if (id != client.clientID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            //changes get set to API service
            var response = await _apiService.UpdateClientAsync(id, client);

            if(response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            //error message if saving changes is unsuccessfull
            ModelState.AddModelError("", "API Error: Failed to save client modifications.");
            return View(client);
        }

        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            //fetching client from database in API
            var client = await _apiService.GetClientByIdAsyc(id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiService.DeleteClientAsync(id);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Delete), new {id = id, error = "Could not delete client."});
        }

    }
}
