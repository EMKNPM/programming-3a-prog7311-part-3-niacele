using Global_Logistics_Management_System___ST10439898.ViewModels;
using Microsoft.AspNetCore.Http; 
using System.Net.Http.Headers;  
using System.Net.Http.Json;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ContractApiService
    {
        private readonly HttpClient _httpClient;
            private readonly IHttpContextAccessor _httpContextAccessor; //for the authentication

        public ContractApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // find token
        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET - all contracts with optional filters
        public async Task<List<ContractViewModel>> GetContractsAsync(string? startDateFrom, string? startDateTo, string? status)
        {
            AttachBearerToken(); // attaches token

            // appends the date info that the user entered to the URL 
            var query = $"?startDateFrom={startDateFrom}&startDateTo={startDateTo}&status={status}";

            var contracts = await _httpClient.GetFromJsonAsync<List<ContractViewModel>>($"api/Contracts{query}");
            return contracts ?? new List<ContractViewModel>();
        }

        // GET - by contract Id
        public async Task<ContractViewModel?> GetContractByIdAsync(int id)
        {
            AttachBearerToken(); 

            try
            {
                return await _httpClient.GetFromJsonAsync<ContractViewModel>($"api/Contracts/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // CREATE - new contract
        public async Task<HttpResponseMessage> CreateContractAsync(ContractViewModel model)
        {
            AttachBearerToken(); 

            using var content = new MultipartFormDataContent();

            // map the fieldsl
            content.Add(new StringContent(model.clientID.ToString()), "clientID");
            content.Add(new StringContent(model.serviceLevel), "serviceLevel");

            // if the file is a pdf, then it gets added to the request
            if (model.pdfUpload != null)
            {
                var fileStream = model.pdfUpload.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                content.Add(streamContent, "pdfUpload", model.pdfUpload.FileName);
            }

            return await _httpClient.PostAsync("api/Contracts", content);
        }

        // UPDATE - update contract
        public async Task<HttpResponseMessage> UpdateContractAsync(int id, ContractViewModel model)
        {
            AttachBearerToken();

            using var content = new MultipartFormDataContent();

            // First, get the existing contract to retrieve the current signedAgreementPath
            var existingContract = await GetContractByIdAsync(id);
            var existingFilePath = existingContract?.signedAgreementPath ?? "";

            Console.WriteLine($"=== SENDING UPDATE TO API ===");
            Console.WriteLine($"contractID: {model.contractID}");
            Console.WriteLine($"clientID: {model.clientID}");
            Console.WriteLine($"startDate: {model.startDate:yyyy-MM-dd}");
            Console.WriteLine($"endDate: {model.endDate:yyyy-MM-dd}");
            Console.WriteLine($"contractStatus (int value): {((int)model.contractStatus).ToString()}");
            Console.WriteLine($"serviceLevel: {model.serviceLevel}");
            Console.WriteLine($"signedAgreementPath (existing): {existingFilePath}");
            Console.WriteLine($"has pdfUpload: {model.pdfUpload != null}");

            content.Add(new StringContent(model.contractID.ToString()), "contractID");
            content.Add(new StringContent(model.clientID.ToString()), "clientID");
            content.Add(new StringContent(model.startDate.ToString("yyyy-MM-dd")), "startDate");
            content.Add(new StringContent(model.endDate.ToString("yyyy-MM-dd")), "endDate");
            content.Add(new StringContent(((int)model.contractStatus).ToString()), "contractStatus");
            content.Add(new StringContent(model.serviceLevel), "serviceLevel");

            // Send the existing file path to satisfy validation
            content.Add(new StringContent(existingFilePath), "signedAgreementPath");

            if (model.pdfUpload != null)
            {
                var fileStream = model.pdfUpload.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(streamContent, "pdfUpload", model.pdfUpload.FileName);
                Console.WriteLine($"Uploading new PDF: {model.pdfUpload.FileName}");
            }
            else
            {
                Console.WriteLine("No new PDF uploaded");
            }

            var response = await _httpClient.PutAsync($"api/Contracts/{id}", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response Status: {response.StatusCode}");
            Console.WriteLine($"API Response Body: {responseBody}");

            return response;
        }
        // DELETE - delete contract
        public async Task<HttpResponseMessage> DeleteContractAsync(int id)
        {
            AttachBearerToken(); 

            return await _httpClient.DeleteAsync($"api/Contracts/{id}");
        }
    }
}