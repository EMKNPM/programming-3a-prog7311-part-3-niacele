using GLMS.API.Data;
using System.Text.Json;
using GLMS.API.Models;

namespace GLMS.API.Services.Behavioural

{
    public class CurrencyConversionObserver : IServiceRequestObserver
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "ea39e0973f82c7a1cf494324";
        private readonly GLMSContext _context;

        public CurrencyConversionObserver(GLMSContext context)
        {
            _httpClient = new HttpClient();
            _context = context;
        }

        public async Task UpdateAsync(ServiceRequest request)
        {
            string apiURL = "https://v6.exchangerate-api.com/v6/ea39e0973f82c7a1cf494324/latest/USD";

            var response = await _httpClient.GetAsync(apiURL);

            if (response.IsSuccessStatusCode)
            {
                //holds the json file with all the conversions from the API
                string jsonString = await response.Content.ReadAsStringAsync();


                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                //finds the ZAR conversion rate
                if (root.GetProperty("result").GetString() == "success")
                {
                    JsonElement rates = root.GetProperty("conversion_rates");
                    decimal zarRate = rates.GetProperty("ZAR").GetDecimal();

                    //calculate the final ZAR cost
                    request.CostinZAR = request.OriginalCost * zarRate;

                    //save converted amount
                    _context.Update(request);
                    await _context.SaveChangesAsync();

                }
            }
        }
    }
}
