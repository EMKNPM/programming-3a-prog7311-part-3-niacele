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

            // map all properties to prepare them to be transmitted through the request
            content.Add(new StringContent(model.contractID.ToString()), "contractID");
            content.Add(new StringContent(model.clientID.ToString()), "clientID");
            content.Add(new StringContent(model.startDate.ToString("o")), "startDate");
            content.Add(new StringContent(model.endDate.ToString("o")), "endDate");
            content.Add(new StringContent(model.contractStatus.ToString()), "contractStatus");
            content.Add(new StringContent(model.serviceLevel), "serviceLevel");
            content.Add(new StringContent(model.signedAgreementPath ?? ""), "signedAgreementPath");

            if (model.pdfUpload != null)
            {
                var fileStream = model.pdfUpload.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                content.Add(streamContent, "pdfUpload", model.pdfUpload.FileName);
            }

            return await _httpClient.PutAsync($"api/Contracts/{id}", content);
        }

        // DELETE - delete contract
        public async Task<HttpResponseMessage> DeleteContractAsync(int id)
        {
            AttachBearerToken(); 

            return await _httpClient.DeleteAsync($"api/Contracts/{id}");
        }
    }
}