using Mapster;
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
        /// <remarks>
        /// Returns a list of all jobs where the associated client belongs to the current user.
        /// </remarks>
        /// <response code="200">Returns the list of jobs</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs()
        {
            var userId = GetCurrentUserId();
            var jobs = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Client.UserId == userId)
                .ProjectToType<JobDto>()
                .ToListAsync();
            return Ok(jobs);
        }

        /// <summary>
        /// Retrieves a specific job by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the job to retrieve.</param>
        /// <remarks>
        /// Returns the job if it exists and belongs to the current user.
        /// </remarks>
        /// <response code="200">Returns the requested job</response>
        /// <response code="404">Job not found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> GetJob(Guid id)
        {
            var userId = GetCurrentUserId();
            var job = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Id == id && j.Client.UserId == userId)
                .ProjectToType<JobDto>()
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
        /// <param name="id">The ID of the job to update.</param>
        /// <param name="jobDto">The updated job object.</param>
        /// <remarks>
        /// Only updates the job if it exists and belongs to the current user.
        /// </remarks>
        /// <response code="204">Job updated successfully</response>
        /// <response code="400">Bad request (ID mismatch or invalid data)</response>
        /// <response code="404">Job not found</response>
        /// <response code="401">Unauthorized</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(Guid id, [FromBody] JobDto jobDto)
        {
            var userId = GetCurrentUserId();
            var existingJob = await _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Id == id && j.Client.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingJob == null)
            {
                return NotFound();
            }

            existingJob.ClientId = jobDto.ClientId;
            existingJob.Title = jobDto.Title;
            existingJob.Description = jobDto.Description;
            existingJob.Status = jobDto.Status;
            if (existingJob.Status == JobStatus.Completed && !jobDto.CompletedAt.HasValue)
            {
                return BadRequest("Cannot set job status to Completed without a completion date.");
            }
            existingJob.CreatedAt = jobDto.CreatedAt;
            if (existingJob.CreatedAt > DateTime.UtcNow)
            {
                return BadRequest("CreatedAt cannot be in the future.");
            }
            existingJob.CompletedAt = jobDto.CompletedAt;
            if (existingJob.CompletedAt.HasValue && existingJob.Status != JobStatus.Completed)
            {
                existingJob.Status = JobStatus.Completed;
            }
            else if (!existingJob.CompletedAt.HasValue && existingJob.Status == JobStatus.Completed)
            {
                existingJob.Status = JobStatus.InProgress;
            }
            existingJob.HourlyRate = jobDto.HourlyRate;
            existingJob.HoursWorked = jobDto.HoursWorked;
            existingJob.MaterialCost = jobDto.MaterialCost;

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
        /// <param name="jobDto">The job object to create.</param>
        /// <remarks>
        /// The client associated with the job must belong to the current user.
        /// </remarks>
        /// <response code="201">Job created successfully</response>
        /// <response code="400">Client does not exist or does not belong to the current user</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob([FromBody] JobDto jobDto)
        {
            var userId = GetCurrentUserId();
            var client = await _context.Clients
                .Where(c => c.Id == jobDto.ClientId && c.UserId == userId)
                .FirstOrDefaultAsync();
            if (client == null)
            {
                return BadRequest("Client does not exist or does not belong to the current user.");
            }
            var job = new Job
            {
                ClientId = jobDto.ClientId,
                Title = jobDto.Title,
                Description = jobDto.Description,
                Status = jobDto.Status,
                CreatedAt = jobDto.CreatedAt,
                CompletedAt = jobDto.CompletedAt,
                HourlyRate = jobDto.HourlyRate,
                HoursWorked = jobDto.HoursWorked,
                MaterialCost = jobDto.MaterialCost,
                UserId = userId
            };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJob", new { id = job.Id }, jobDto);
        }

        /// <summary>
        /// Deletes a specific job by ID for the logged in user.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <remarks>
        /// Only deletes the job if it exists and belongs to the current user.
        /// </remarks>
        /// <response code="204">Job deleted successfully</response>
        /// <response code="404">Job not found</response>
        /// <response code="401">Unauthorized</response>
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