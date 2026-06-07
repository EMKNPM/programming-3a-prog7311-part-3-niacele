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
        public async Task<List<Client>> GetClientAsync()
        {
            //send a GET request to the API
            var clients = await _httpclient.GetAllClientsAsync<List<Client>>(""); //retrieves JSON data and converts it to a C# list

            return clients ?? new List<Client>();
        }

        //GET - specific client by ID
        public async Task<Client?> GetClientByIdAsyc (int id)
        {
            try
            {
                //append ID onto URL path 
                return await _httpclient.GetFromJsonAsync<Client>($"{id}");

            }

            catch (HttpRequestException)
            {
                //returns null if client isnt found
                return null;
            }
        }

        //CREATE - new client
        public async Task<HttpResponseMessage> UpdateClientAsync (int id, Client client) //return type is HttpResponseMessage rather than Client object - method can display correct alerts
        {
            //converts C# to JSON so that the API can understand
            return await _httpclient.PostAsJsonAsync("", client);

        }

        //UPDATE - update client
        public async Task<HttpResponseMessage> UpdateClientAsync(int id, ClientApiService client)
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
