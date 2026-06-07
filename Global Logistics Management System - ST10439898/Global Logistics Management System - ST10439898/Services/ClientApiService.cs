using Microsoft.AspNetCore.Mvc;
using Global_Logistics_Management_System___ST10439898.ViewModels;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ClientApiService
    {
        private readonly HttpClient _httpclient; 

        //constructor for dependency injection
        public ClientApiService (HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        //GET - all clients
        public async Task<List<ClientViewModel>> GetClientAsync()
        {
            //send a GET request to the API
            var clients = await _httpclient.GetFromJsonAsync<List<ClientViewModel>>("");
            
            return clients ?? new List<ClientViewModel>();
        }

        //GET - specific client by ID
        public async Task<ClientViewModel?> GetClientByIdAsyc (int id)
        {
            try
            {
                //append ID onto URL path 
                return await _httpclient.GetFromJsonAsync<ClientViewModel>($"{id}");

            }

            catch (HttpRequestException)
            {
                //returns null if client isnt found
                return null;
            }
        }

        //CREATE - new client
        public async Task<HttpResponseMessage> CreateClientAsync(int id, ClientViewModel client) //return type is HttpResponseMessage rather than Client object - method can display correct alerts
        {
            //converts C# to JSON so that the API can understand
            return await _httpclient.PostAsJsonAsync("", client);

        }

        //UPDATE - update client
        public async Task<HttpResponseMessage> UpdateClientAsync(int id, ClientViewModel client)
        {
            //converts C# to JSON so that the API can understand
            return await _httpclient.PutAsJsonAsync($"{id}", client);

        }

        //DELETE - delete client
        public async Task<HttpResponseMessage> DeleteClientAsync (int id)
        {
            //converts C# to JSON so that the API can understand
            return await _httpclient.DeleteAsync($"{id}");
        }


    }
}
