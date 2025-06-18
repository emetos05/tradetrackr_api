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
    public class JobsController : ControllerBase
    {
        private readonly TradeTrackrDbContext _context;

        public JobsController(TradeTrackrDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId() => HttpContext?.User?.FindFirst("sub")?.Value;

        /// <summary>
        /// Retrieves all jobs for the logged in user.
        /// </summary>
        /// <returns></returns>
        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            var userId = GetCurrentUserId();
            // Only return jobs where the job's client belongs to the current user
            return await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Client.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific job by ID for the logged in user.
        /// </summary>
        /// <returns></returns>
        // GET: api/Jobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(Guid id)
        {
            var userId = GetCurrentUserId();
            var job = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Id == id && j.Client.UserId == userId)
                .FirstOrDefaultAsync();

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        /// <summary>
        /// Updates an existing job for the logged in user.
        /// </summary>
        /// <returns></returns>
        // PUT: api/Jobs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(Guid id, Job job)
        {
            var userId = GetCurrentUserId();
            if (id != job.Id)
            {
                return BadRequest();
            }

            // Ensure the job belongs to the current user
            var existingJob = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Id == id && j.Client.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingJob == null)
            {
                return NotFound();
            }

            // Only update the fields that are allowed to be changed
            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.HourlyRate = job.HourlyRate;
            existingJob.HoursWorked = job.HoursWorked;
            existingJob.MaterialCost = job.MaterialCost;
            existingJob.ClientId = job.ClientId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(id))
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
        /// Creates a new job for the logged in user.
        /// </summary>
        /// <returns></returns>
        // POST: api/Jobs
        [HttpPost]
        public async Task<ActionResult<Job>> CreateJob(Job job)
        {
            var userId = GetCurrentUserId();
            // Ensure the client belongs to the current user
            var client = await _context.Clients
                .Where(c => c.Id == job.ClientId && c.UserId == userId)
                .FirstOrDefaultAsync();
            if (client == null)
            {
                return BadRequest("Client does not exist or does not belong to the current user.");
            }
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJob", new { id = job.Id }, job);
        }

        /// <summary>
        /// Deletes a specific job by ID for the logged in user.
        /// </summary>
        /// <returns></returns>
        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            var userId = GetCurrentUserId();
            var job = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Id == id && j.Client.UserId == userId)
                .FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JobExists(Guid id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
    }
}
