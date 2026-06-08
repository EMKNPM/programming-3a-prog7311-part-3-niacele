using Microsoft.AspNetCore.Mvc;
using Global_Logistics_Management_System___ST10439898.ViewModels;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ClientApiService
    {
        private readonly HttpClient _httpclient;

        // constructor for dependency injection
        public ClientApiService(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        // GET - all clients
        public async Task<List<ClientViewModel>> GetClientsAsync()
        {
            // send a GET request to the API - matches [HttpGet] in ClientsController
            var clients = await _httpclient.GetFromJsonAsync<List<ClientViewModel>>("api/Clients");

            return clients ?? new List<ClientViewModel>();
        }

        // GET - specific client by ID
        public async Task<ClientViewModel?> GetClientByIdAsync(int id)
        {
            try
            {
                // append ID onto URL path - matches [HttpGet("{id}")] in ClientsController
                return await _httpclient.GetFromJsonAsync<ClientViewModel>($"api/Clients/{id}");
            }
            catch (HttpRequestException)
            {
                // returns null if client isn't found
                return null;
            }
        }

        // CREATE - new client
        public async Task<HttpResponseMessage> CreateClientAsync(ClientViewModel client) 
        {
            // converts C# to JSON so that the API can understand - matches [HttpPost] in ClientsController
            return await _httpclient.PostAsJsonAsync("api/Clients", client);
        }

        // UPDATE - update client
        public async Task<HttpResponseMessage> UpdateClientAsync(int id, ClientViewModel client)
        {
            // converts C# to JSON so that the API can understand - matches [HttpPut] in ClientsController
            return await _httpclient.PutAsJsonAsync($"api/Clients/{id}", client);
        }

        // DELETE - delete client
        public async Task<HttpResponseMessage> DeleteClientAsync(int id)
        {
            // matches [HttpDelete("{id}")] in ClientsController
            return await _httpclient.DeleteAsync($"api/Clients/{id}");
        }
    }
}