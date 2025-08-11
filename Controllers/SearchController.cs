using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tradetrackr.api.Dto;
using tradetrackr.api.Services;

namespace tradetrackr.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        private string GetCurrentUserId() => HttpContext?.User?.FindFirst("sub")?.Value ?? string.Empty;

        /// <summary>
        /// Performs a global search across clients, jobs, and invoices for the authenticated user.
        /// </summary>
        /// <param name="request">The search request containing query and filters.</param>
        /// <remarks>
        /// Searches across multiple fields in clients (name, email, phone, address), 
        /// jobs (title, description, status), and invoices (status, notes, terms, amount).
        /// 
        /// The search supports:
        /// - Case-insensitive matching
        /// - Multiple search terms
        /// - Quoted phrases for exact matching
        /// - Field highlighting in results
        /// - Filtering by entity types
        /// - Configurable maximum results per entity type
        /// 
        /// Examples:
        /// - "john doe" - searches for the exact phrase "john doe"
        /// - smith electrician - searches for records containing both "smith" and "electrician"
        /// - "ABC Company" plumbing - searches for "ABC Company" (exact) and "plumbing"
        /// </remarks>
        /// <response code="200">Returns the search results</response>
        /// <response code="400">If the search request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost]        
        public async Task<ActionResult<GlobalSearchResponse>> Search([FromBody] GlobalSearchRequest request)
        {
            if (request == null)
            {
                return BadRequest("Search request is required");
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Ok(new GlobalSearchResponse { Query = request.Query });
            }

            if (request.MaxResults <= 0)
            {
                request.MaxResults = 50; // Default value
            }

            if (request.MaxResults > 200)
            {
                return BadRequest("Maximum results cannot exceed 200");
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _searchService.SearchAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception here in a real application
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing the search request");
            }
        }

        /// <summary>
        /// Performs a simple global search with just a query string.
        /// </summary>
        /// <param name="q">The search query</param>
        /// <param name="maxResults">Maximum number of results to return (default: 50, max: 200)</param>
        /// <param name="entityTypes">Comma-separated list of entity types to search (Clients,Jobs,Invoices). If empty, searches all.</param>
        /// <remarks>
        /// Simplified search endpoint that accepts query parameters instead of a request body.
        /// Useful for quick searches and URL-based searches.
        /// 
        /// Example: GET /api/search?q=john%20smith&amp;maxResults=25&amp;entityTypes=Clients,Jobs
        /// </remarks>
        /// <response code="200">Returns the search results</response>
        /// <response code="400">If the search parameters are invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(GlobalSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GlobalSearchResponse>> SearchGet(
            [FromQuery] string q,
            [FromQuery] int maxResults = 50,
            [FromQuery] string? entityTypes = null)
        {
            var request = new GlobalSearchRequest
            {
                Query = q ?? string.Empty,
                MaxResults = maxResults
            };

            // Parse entity types if provided
            if (!string.IsNullOrWhiteSpace(entityTypes))
            {
                var typesList = new List<SearchEntity>();
                var types = entityTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var type in types)
                {
                    if (Enum.TryParse<SearchEntity>(type.Trim(), true, out var entityType))
                    {
                        typesList.Add(entityType);
                    }
                }
                
                if (typesList.Count > 0)
                {
                    request.EntityTypes = typesList.ToArray();
                }
            }

            return await Search(request);
        }
    }
}