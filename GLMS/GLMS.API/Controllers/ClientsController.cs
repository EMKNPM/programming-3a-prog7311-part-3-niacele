using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GLMS.API.Data;
using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly GLMSContext _context;

        //constructor for dependency injection
        public ClientsController(GLMSContext context)
        {
            _context = context;
        }

        //GET - all clients
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            //query to fetch all clients from database asynchronously
            var clientsQuery = await _context.Clients.ToListAsync();

            //return Json data
            return Ok(clientsQuery);
        }

        //GET - specific clients according to ID
        [HttpGet("{id}")] //client ID gets appended to the URL path
        public async Task<IActionResult> GetClientById(int id)
        {
            //search database for client with matching ID
            var clientQuery = await _context.Clients.FirstOrDefaultAsync(c => c.clientID == id);

            if (clientQuery == null)
            {
                //return error message if ID doesnt currently exist in database
                return NotFound(new { message = $"Client Retrieval failed: Client with ID {id} does not exist." });

            }

            //return JSON data
            return Ok(clientQuery);
        }

        //POST - create brand new client
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            //checks whether the data matches validation rules
            if(!ModelState.IsValid)
            {
                //grabs all validation errors 
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);

                //return error message
                return BadRequest(new { message = "Validation failed for client data insertion.", validationErrors = errors });
            }

            //add new client to database + save changes made to database
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClientById), new { id = client.clientID }, client);
        }

        //PUT - update existing clients
        [HttpPut]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] Client client)
        {
            //ensures that ID thats passed through URL path + JSON body payload are the same - prevents from tampering or bugs
            if(id != client.clientID)
            {
                return BadRequest(new { message = "Update rejected: URL ID parameter does not match internal ID" });
            }

            //makes data integrity rules are being followed
            if(!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);

                return BadRequest(new { message = "Validation failed for client update properties.", validationErrors = errors });
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                //saves changes made to database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //because we are using Docker, there could be multiple users working on the same record at once (concurrency) - need to check to make sure that no one has deleted the client thats being updated
                if (!_context.Clients.Any(e => e.clientID == id))
                {
                    return NotFound(new { message = $"Update failed: Client with ID {id} no longer exists in the system." });
                }
                else
                {
                    //throws error if some other database issue is at hand
                    throw; 
                }
            }

            //returns JSON data
            return Ok(client);
        }

       //DELETE - delete a specific client
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            //searches database for correct client using ID
            var clientQuery = await _context.Clients.FindAsync(id);

            if (clientQuery == null)
            {
                //return an error message if client with this ID doesnt exist
                return NotFound(new { message = $"Delete operation failed: Client with ID {id} was not found." });
            }

            //delete client from database
            _context.Clients.Remove(clientQuery);

            //save changes made to database
            await _context.SaveChangesAsync();

            //return JSON data
            return Ok(new { message = $"Client account {id} successfully purged from system records." });
        }
    }
}
