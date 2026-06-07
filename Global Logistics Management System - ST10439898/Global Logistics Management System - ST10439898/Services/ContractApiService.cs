using Global_Logistics_Management_System___ST10439898.ViewModels;

namespace Global_Logistics_Management_System___ST10439898.Services
{
    public class ContractApiService
    {
        private readonly HttpClient httpClient;

        public ContractApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //GET - all contracts
        public async Task<List<ContractViewModel>> GetContractsAsync(string? startDateFrom, string? startDateTo, string? status)
        {
            //appends the date info that the user entered to the URL 
            var query = $"?startDateFrom={startDateFrom}&startDateTo={startDateTo}&status={status}";
            var contracts = await _httpClient.GetFromJsonAsync<List<ContractViewModel>>(query);
            return contracts ?? new List<ContractViewModel>();
        }

        //GET - by contract Id
        public async Task<ContractViewModel?> GetContractByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ContractViewModel>($"{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        //CREATE - new contract
        public async Task<HttpResponseMessage> CreateContractAsync(ContractViewModel model)
        {
            using var content = new MultipartFormDataContent();

            //map the fields
            content.Add(new StringContent(model.ClientID.ToString()), "clientID");
            content.Add(new StringContent(model.ServiceLevel), "serviceLevel");

            //if the file is a pdf, then it gets added to the request
            if (model.PdfUpload != null)
            {
                var fileStream = model.PdfUpload.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                content.Add(streamContent, "pdfUpload", model.PdfUpload.FileName);
            }

            return await _httpClient.PostAsync("", content);
        }

        //UPDATE - update contract
        public async Task<HttpResponseMessage> UpdateContractAsync(int id, ContractViewModel model)
        {
            using var content = new MultipartFormDataContent();

            //map all properties to prepare them to be transmitted through the request
            content.Add(new StringContent(model.ContractID.ToString()), "contractID");
            content.Add(new StringContent(model.ClientID.ToString()), "clientID");
            content.Add(new StringContent(model.StartDate.ToString("o")), "startDate"); 
            content.Add(new StringContent(model.EndDate.ToString("o")), "endDate");
            content.Add(new StringContent(model.ContractStatus), "contractStatus");
            content.Add(new StringContent(model.ServiceLevel), "serviceLevel");
            content.Add(new StringContent(model.SignedAgreementPath ?? ""), "signedAgreementPath");

            if (model.PdfUpload != null)
            {
                var fileStream = model.PdfUpload.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                content.Add(streamContent, "pdfUpload", model.PdfUpload.FileName);
            }

            return await _httpClient.PutAsync($"{id}", content);
        }

        //DELETE - delete contract
        public async Task<HttpResponseMessage> DeleteContractAsync(int id)
        {
            return await _httpClient.DeleteAsync($"{id}");
        }

    }
}
