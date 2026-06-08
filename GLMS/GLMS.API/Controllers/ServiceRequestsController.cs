using GLMS.API.Data; 
using GLMS.API.Models; 
using GLMS.API.Services.Behavioural;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GLMS.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly GLMSContext _context;
        private readonly ServiceRequestManager _manager;
        private readonly IHttpClientFactory _httpClientFactory;

        //constructor for dependency injection 
        public ServiceRequestsController (GLMSContext context, ServiceRequestManager manager, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _manager = manager;
            _httpClientFactory = httpClientFactory;
        }


        //GET - all service requests
        [HttpGet]
        public async Task<IActionResult> GetServiceRequests()
        {
            //fetches all the service requests from the server  
            var requestsQuery = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .ToListAsync();

            //returns JSON data 
            return Ok(requestsQuery);
        }

        //GET - specific service requests
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceRequestById(int id)
        {
            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.requestID == id);

            //checks that ID field isnt empty
            if (serviceRequest == null)
            {
                //return appropriate error message
                return NotFound(new { message = $"Search failed: Service Request {id} does not exist." }); // HTTP 404
            }

            return Ok(serviceRequest);
        }

        
        //POST - create new service request
        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequest serviceRequest)
        {
            //checks that validation rules have been met
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation processing errors found.", validationErrors = errors }); // HTTP 400
            }

            //query to see if parent contract exists in database
            var parentContract = await _context.Contracts.FindAsync(serviceRequest.contractID);
            if (parentContract == null)
            {
                //appropriate error message if contract doesnt exist
                return NotFound(new { message = "Action Denied: The specified parent contract could not be found." });
            }

            //have to sync in order to make sure that the status works with the design patterns and works correctly
            parentContract.SyncStateFromStatus();

            try
            {
               //only contracts with active status can have a service request put in place
                await _manager.CreateServiceRequestAsync(parentContract, serviceRequest);

                //returns JSON data
                return CreatedAtAction(nameof(GetServiceRequestById), new { id = serviceRequest.requestID }, serviceRequest);
            }
            catch (InvalidOperationException ex)
            {
                //returned only in contract status doesnt allow for service requests (ie. anything other than active)
                return BadRequest(new { message = "State Workflow Exception", detail = ex.Message });
            }
        }

        //POST - edit service requests
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRequest(int id, [FromBody] ServiceRequest serviceRequest)
        {
            //safety check - IDs should match in both places
            if (id != serviceRequest.requestID)
            {
                return BadRequest(new { message = "ID mismatch encountered between URL route data and payload content." });
            }

            //validation check
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation errors found on modification properties.", validationErrors = errors });
            }

            //checks that request with that ID exists in the first place
            var existingRequest = await _context.ServiceRequests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound(new { message = $" Service Request Update failed: Target Service Request {id} not found." });
            }

            //replaces existing record with new information
            existingRequest.contractID = serviceRequest.contractID;
            existingRequest.Description = serviceRequest.Description;
            existingRequest.requestStatus = serviceRequest.requestStatus;

            _context.Entry(existingRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ServiceRequests.Any(e => e.requestID == id))
                {
                    return NotFound(new { message = "Data modification collision: Record was deleted simultaneously." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingRequest);
        }

        //DELETE - delete a service request
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            //finds service request with that ID
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound(new { message = $"Delete processing cancelled: Request {id} missing." });
            }

            //deletes request from database
            _context.ServiceRequests.Remove(serviceRequest);

            //save changes
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Service Request {id} successfully eliminated from database storage." });
        }

        //GET - for CURRENCY CONVERTER  
        [HttpGet("convert-currency")] //custom path 
        public async Task<IActionResult> ConvertCurrency([FromQuery] decimal amount, [FromQuery] string fromCurrency)
        {
            try
            {
                //third party link
                string apiURL = $"https://v6.exchangerate-api.com/v6/2bf5d5097fb478f28bc706e4/latest/{fromCurrency}";

                //communicate with third part api for conversion rates
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(apiURL);

                if (response.IsSuccessStatusCode)
                {
                    //takes the conversion rate and stores it
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    if (root.GetProperty("result").GetString() == "success")
                    {
                        JsonElement rates = root.GetProperty("conversion_rates");
                        decimal zarRate = rates.GetProperty("ZAR").GetDecimal();
                        decimal convertedAmount = amount * zarRate;

                        //returns converted amount
                        return Ok(new { success = true, zarAmount = convertedAmount });
                    }
                }

                return BadRequest(new { success = false, message = "External exchange rate provider connection failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }
    }
}