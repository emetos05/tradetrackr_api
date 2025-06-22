using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tradetrackr.api.Data;
using tradetrackr.api.Dto;
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

        public ClientsController(TradeTrackrDbContext context)
        {
            _context = context;
        }

        // For M2M (machine-to-machine) Auth0, use "client_id" claim instead of "sub"
        private string GetCurrentUserId()
        {
            // Try to get the client_id claim (for M2M)
            var userId = HttpContext.User.FindFirst("client_id")?.Value;

            userId = "1234"; // For testing

            if (!string.IsNullOrEmpty(userId))
                return userId;
            // Fallback to sub (for user-based tokens)
            return HttpContext.User.FindFirst("sub")?.Value;
        }

        /// <summary>
        /// Retrieves all clients for the logged in user.
        /// </summary>
        /// <remarks>
        /// Returns a list of all clients associated with the authenticated user.
        /// </remarks>
        /// <response code="200">Returns the list of clients</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var userId = GetCurrentUserId();
            var clients = await _context.Clients.Where(c => c.UserId == userId)
                .Select(c => new ClientDto
                {
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address
                })
                .ToListAsync();
            return clients;
        }

        /// <summary>
        /// Retrieves a specific client by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the client to retrieve.</param>
        /// <remarks>
        /// Returns the client with the specified ID if it belongs to the authenticated user.
        /// </remarks>
        /// <response code="200">Returns the requested client</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the client is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientDto>> GetClient(Guid id)
        {
            var userId = GetCurrentUserId();
            var client = await _context.Clients
                .Where(c => c.Id == id && c.UserId == userId)
                .Select(c => new ClientDto
                {
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address
                })
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
        /// <param name="id">The ID of the client to update.</param>
        /// <param name="clientDto">The updated client object.</param>
        /// <remarks>
        /// Updates the specified client if it belongs to the authenticated user.
        /// </remarks>
        /// <response code="204">Client updated successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the client is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] ClientDto clientDto)
        {
            var userId = GetCurrentUserId();

            var existingClient = await _context.Clients
                .Where(c => c.Id == id && c.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingClient == null)
            {
                return NotFound();
            }

            existingClient.Name = clientDto.Name;
            existingClient.Email = clientDto.Email;
            existingClient.Phone = clientDto.Phone;
            existingClient.Address = clientDto.Address;

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
        /// <param name="clientDto">The client object to create.</param>
        /// <remarks>
        /// Adds a new client associated with the authenticated user.
        /// </remarks>
        /// <response code="201">Client created successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateClient([FromBody] ClientDto clientDto)
        {
            var client = new Client
            {
                Name = clientDto.Name,
                Email = clientDto.Email,
                Phone = clientDto.Phone,
                Address = clientDto.Address,
                UserId = GetCurrentUserId()
            };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, clientDto);
        }

        /// <summary>
        /// Deletes a specific client by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the client to delete.</param>
        /// <remarks>
        /// Deletes the client with the specified ID if it belongs to the authenticated user.
        /// </remarks>
        /// <response code="204">Client deleted successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the client is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
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