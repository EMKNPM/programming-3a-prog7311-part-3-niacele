using Global_Logistics_Management_System___ST10439898.ViewModels;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ServiceRequestApiService
    {
        private readonly HttpClient _httpClient;

        public ServiceRequestApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //GET - all service requests
        public async Task<List<ServiceRequestApiService>> GetServiceRequestsAsync()
        {
            var requests = await _httpClientGetFromJsonAsync<List<ServiceRequestViewModel>>("");
            return requests ?? new List<ServiceRequestViewModel>(); //if there arent any service requests, new list will be instantiated
        }

        //GET - specific service request
        public async Task<ServiceRequestViewModel?> GetServiceRequestById(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ServiceRequestViewModel>($"{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        //CREATE - create new service request
        public async Task<HttpResponseMessage> CreateServiceRequestAsync(ServiceRequestViewModel serviceRequest)
        {
            return await _httpClient.PostAsJsonAsync("", serviceRequest);
        }

        //UPDATE - update service request
        public async Task<HttpResponseMessage> UpdateServiceRequestAsync(int id, ServiceRequestViewModel serviceRequest)
        {
            return await _httpClient.PutAsJsonAsync($"{id}", serviceRequest);
        }

        //DELETE - delete service request
        public async Task<HttpResponseMessage> DeleteServiceRequestAsync(int id)
        {
            return await _httpClient.DeleteAsync($"{id}");
        }

        //method to connect to Currency Converter
        public async Task<CurrencyConversionResult?> ConvertCurrencyAsync(decimal amount, string fromCurrency)
        {
            try
            {
                //forwards info to the the currency converter through the API
                string queryPath = $"convert-currency?amount={amount}&fromCurrency={fromCurrency}";
                return await _httpClient.GetFromJsonAsync<CurrencyConversionResult>(queryPath);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

    }

    //helps convert the JSON result we get from the API into plain text
    public class CurrencyConversionResult
    {
        public bool Success { get; set; }
        public decimal ZarAmount { get; set; }
        public string? Message { get; set; }
    }
}
