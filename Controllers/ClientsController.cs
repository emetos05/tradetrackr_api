using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Data;
using tradetrackr.api.Models;

namespace tradetrackr.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly TradeTrackrDbContext _context;

        public ClientsController(TradeTrackrDbContext context )
        {
            _context = context;
        }

        // For M2M (machine-to-machine) Auth0, use "client_id" claim instead of "sub"
        private string GetCurrentUserId()
        {
            // Try to get the client_id claim (for M2M)
            var clientId = HttpContext.User.FindFirst("client_id")?.Value;
            if (!string.IsNullOrEmpty(clientId))
                return clientId;
            // Fallback to sub (for user-based tokens)
            return HttpContext.User.FindFirst("sub")?.Value;
        }

        /// <summary>
        /// Retrieves all clients for the logged in user.
        /// </summary>
        /// <returns></returns>
        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            // Optionally filter by the current user if needed
            var userId = GetCurrentUserId();
            //var userId = "google-oauth2|116105830163052709919"; // For testing purposes, replace with actual user ID retrieval logic

            return await _context.Clients.Where(c => c.UserId == userId).ToListAsync();           
        }
                
        /// <summary>
        /// Retrieves a specific client by ID for the logged in user.
        /// </summary>
        /// <returns></returns>
        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(Guid id)
        {
            var userId = GetCurrentUserId();
            
            // Retrieve the client by ID and ensure it belongs to the current user
            var client = await _context.Clients
                .Where(c => c.Id == id && c.UserId == userId)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        /// <summary>
        /// Updates an existing client for the logged in user.
        /// </summary>
        /// <returns></returns>
        // PUT: api/Clients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(Guid id, Client client)
        {
            var userId = GetCurrentUserId();
            
            if (id != client.Id)
            {
                return BadRequest();
            }
            
            // Ensure the client belongs to the current user
            var existingClient = await _context.Clients
                .Where(c => c.Id == id && c.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingClient == null)
            {
                return NotFound();
            }

            // Only update the fields that are allowed to be changed
            existingClient.Name = client.Name;
            existingClient.Email = client.Email;
            existingClient.Phone = client.Phone;
            existingClient.Address = client.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new client for the logged in user.
        /// </summary>
        /// <returns></returns>
        // POST: api/Clients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            client.UserId = GetCurrentUserId(); 
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        /// <summary>
        /// Deletes a specific client by ID for the logged in user.
        /// </summary>
        /// <returns></returns>
        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            // Ensure the client belongs to the current user
            var client = await _context.Clients
                .Where(c => c.Id == id && c.UserId == userId)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientExists(Guid id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
