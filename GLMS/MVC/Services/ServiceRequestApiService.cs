using Global_Logistics_Management_System___ST10439898.ViewModels;
using System.Net.Http.Headers;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ServiceRequestApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceRequestApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachBearerToken()
        {
            // grabs active token
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET - all service requests
        public async Task<List<ServiceRequestViewModel>> GetServiceRequestsAsync()
        {
            AttachBearerToken();

            var requests = await _httpClient.GetFromJsonAsync<List<ServiceRequestViewModel>>("api/ServiceRequests");
            return requests ?? new List<ServiceRequestViewModel>();
        }

        // GET - specific service request
        public async Task<ServiceRequestViewModel?> GetServiceRequestByIdAsync(int? id)
        {
            AttachBearerToken();

            if (id == null) return null;
            try
            {
                return await _httpClient.GetFromJsonAsync<ServiceRequestViewModel>($"api/ServiceRequests/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // CREATE - create new service request
        public async Task<HttpResponseMessage> CreateServiceRequestAsync(ServiceRequestViewModel serviceRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ServiceRequests", serviceRequest);
            return await _httpClient.PostAsJsonAsync("api/ServiceRequests", serviceRequest);
        }

        // UPDATE - update service request
        public async Task<HttpResponseMessage> UpdateServiceRequestAsync(int id, ServiceRequestViewModel serviceRequest)
        {
            AttachBearerToken();

            return await _httpClient.PutAsJsonAsync($"api/ServiceRequests/{id}", serviceRequest);
        }

        // DELETE - delete service request
        public async Task<HttpResponseMessage> DeleteServiceRequestAsync(int id)
        {
            AttachBearerToken();

            return await _httpClient.DeleteAsync($"api/ServiceRequests/{id}");
        }

        // method to connect to Currency Converter
        public async Task<CurrencyConversionResult?> ConvertCurrencyAsync(decimal amount, string fromCurrency)
        {
            AttachBearerToken();

            try
            {
                // forwards info to the currency converter through the API
                string queryPath = $"api/ServiceRequests/convert-currency?amount={amount}&fromCurrency={fromCurrency}";
                return await _httpClient.GetFromJsonAsync<CurrencyConversionResult>(queryPath);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }

    // helps convert the JSON result we get from the API into plain text
    public class CurrencyConversionResult
    {
        public bool Success { get; set; }
        public decimal ZarAmount { get; set; }
        public string? Message { get; set; }
    }
}