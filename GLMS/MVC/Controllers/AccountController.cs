using Microsoft.AspNetCore.Mvc;
using Global_Logistics_Management_System___ST10439898.ViewModels;
using Microsoft.Extensions.Logging;

namespace Global_Logistics_Management_System___ST10439898.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
        {
            // pulls pre configured client
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        // GET - Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST - Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            //logger
            _logger.LogInformation("Frontend: Attempting login call to backend API for user: {Username}", model.Username);

            // passing the credentials to the api
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7048/api/Auth/login", model);

            //logger
            _logger.LogInformation("Frontend: Backend API responded with Status Code: {StatusCode}", response.StatusCode);

            try
            {

                if (response.IsSuccessStatusCode)
                {
                    // get the token
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseResult>();

                    // take the token
                    HttpContext.Session.SetString("JWToken", result.Token);
                    HttpContext.Session.SetString("StaffName", result.FullName);

                    //logger
                    _logger.LogInformation("Frontend: Login Successful! Token stored in session for {User}", result.FullName);
                    // go to the contract page
                    return RedirectToAction("Index", "Contract");
                }
            }
            catch (Exception ex)
            {
                // logger
                _logger.LogError(ex, "Frontend CRITICAL: Failed to communicate with the backend API network port.");
            }

            ModelState.AddModelError("", "Access Denied: Invalid staff username or password.");
            return View(model);
        }
        

        // GET - Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // clears out tokens
            return RedirectToAction("Login");
        }
    }

    public class LoginResponseResult
    {
        public string Token { get; set; }
        public string FullName { get; set; }
    }
}