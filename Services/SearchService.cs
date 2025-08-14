using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using tradetrackr.api.Data;
using tradetrackr.api.Dto;
using tradetrackr.api.Models;

namespace tradetrackr.api.Services
{
    public class SearchService : ISearchService
    {
        private readonly TradeTrackrDbContext _context;

        public SearchService(TradeTrackrDbContext context)
        {
            _context = context;
        }

        public async Task<GlobalSearchResponse> SearchAsync(GlobalSearchRequest request, string userId)
        {
            var response = new GlobalSearchResponse
            {
                Query = request.Query
            };

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return response;
            }

            var searchTerms = ParseSearchQuery(request.Query);
            var entityTypes = request.EntityTypes ?? Enum.GetValues<SearchEntity>();

            // Run search operations sequentially to avoid DbContext concurrency issues
            if (entityTypes.Contains(SearchEntity.Clients))
            {
                await SearchClientsAsync(searchTerms, userId, request.MaxResults, response.Results);
            }

            if (entityTypes.Contains(SearchEntity.Jobs))
            {
                await SearchJobsAsync(searchTerms, userId, request.MaxResults, response.Results);
            }

            if (entityTypes.Contains(SearchEntity.Invoices))
            {
                await SearchInvoicesAsync(searchTerms, userId, request.MaxResults, response.Results);
            }

            response.TotalResults = response.Results.Clients.Count + 
                                  response.Results.Jobs.Count + 
                                  response.Results.Invoices.Count;

            return response;
        }

        private static string[] ParseSearchQuery(string query)
        {
            // Split by spaces and remove empty entries, handle quoted strings
            var terms = new List<string>();
            var matches = Regex.Matches(query, @"""([^""]*)""|(\S+)");
            
            foreach (Match match in matches)
            {
                var term = (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim();
                if (!string.IsNullOrEmpty(term))
                {
                    terms.Add(term);
                }
            }

            return terms.ToArray();
        }

        private async Task SearchClientsAsync(string[] searchTerms, string userId, int maxResults, SearchResults results)
        {
            var query = _context.Clients
                .Where(c => c.UserId == userId)
                .AsQueryable();

            var matchingClients = new List<(Client Client, List<string> MatchedFields, string Highlight)>();

            var clients = await query.ToListAsync();

            foreach (var client in clients)
            {
                var matchedFields = new List<string>();
                var highlights = new List<string>();

                // Search in various fields
                if (ContainsAnyTerm(client.Name, searchTerms))
                {
                    matchedFields.Add(nameof(client.Name));
                    highlights.Add($"Name: {HighlightMatches(client.Name, searchTerms)}");
                }

                if (ContainsAnyTerm(client.Email, searchTerms))
                {
                    matchedFields.Add(nameof(client.Email));
                    highlights.Add($"Email: {HighlightMatches(client.Email, searchTerms)}");
                }

                if (ContainsAnyTerm(client.Phone, searchTerms))
                {
                    matchedFields.Add(nameof(client.Phone));
                    highlights.Add($"Phone: {HighlightMatches(client.Phone, searchTerms)}");
                }

                if (ContainsAnyTerm(client.Address, searchTerms))
                {
                    matchedFields.Add(nameof(client.Address));
                    highlights.Add($"Address: {HighlightMatches(client.Address, searchTerms)}");
                }

                if (matchedFields.Count > 0)
                {
                    matchingClients.Add((client, matchedFields, string.Join(" | ", highlights)));
                }
            }

            results.Clients = matchingClients
                .Take(maxResults)
                .Select(match => new SearchResultItem<ClientDto>
                {
                    Item = match.Client.Adapt<ClientDto>(),
                    EntityType = SearchEntity.Clients,
                    MatchedFields = match.MatchedFields,
                    Highlight = match.Highlight
                })
                .ToList();
        }

        private async Task SearchJobsAsync(string[] searchTerms, string userId, int maxResults, SearchResults results)
        {
            var query = _context.Jobs
                .Include(j => j.Client)
                .Where(j => j.Client.UserId == userId)
                .AsQueryable();

            var matchingJobs = new List<(Job Job, List<string> MatchedFields, string Highlight)>();

            var jobs = await query.ToListAsync();

            foreach (var job in jobs)
            {
                var matchedFields = new List<string>();
                var highlights = new List<string>();

                if (ContainsAnyTerm(job.Title, searchTerms))
                {
                    matchedFields.Add(nameof(job.Title));
                    highlights.Add($"Title: {HighlightMatches(job.Title, searchTerms)}");
                }

                if (ContainsAnyTerm(job.Description, searchTerms))
                {
                    matchedFields.Add(nameof(job.Description));
                    highlights.Add($"Description: {HighlightMatches(job.Description, searchTerms)}");
                }

                if (ContainsAnyTerm(job.Status.ToString(), searchTerms))
                {
                    matchedFields.Add(nameof(job.Status));
                    highlights.Add($"Status: {HighlightMatches(job.Status.ToString(), searchTerms)}");
                }

                // Search in related client name
                if (ContainsAnyTerm(job.Client?.Name, searchTerms))
                {
                    matchedFields.Add("ClientName");
                    highlights.Add($"Client: {HighlightMatches(job.Client.Name, searchTerms)}");
                }

                if (matchedFields.Count > 0)
                {
                    matchingJobs.Add((job, matchedFields, string.Join(" | ", highlights)));
                }
            }

            results.Jobs = matchingJobs
                .Take(maxResults)
                .Select(match => new SearchResultItem<JobDto>
                {
                    Item = match.Job.Adapt<JobDto>(),
                    EntityType = SearchEntity.Jobs,
                    MatchedFields = match.MatchedFields,
                    Highlight = match.Highlight
                })
                .ToList();
        }

        private async Task SearchInvoicesAsync(string[] searchTerms, string userId, int maxResults, SearchResults results)
        {
            var query = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Job)
                .Where(i => i.UserId == userId)
                .AsQueryable();

            var matchingInvoices = new List<(Invoice Invoice, List<string> MatchedFields, string Highlight)>();

            var invoices = await query.ToListAsync();

            foreach (var invoice in invoices)
            {
                var matchedFields = new List<string>();
                var highlights = new List<string>();

                if (ContainsAnyTerm(invoice.Status.ToString(), searchTerms))
                {
                    matchedFields.Add(nameof(invoice.Status));
                    highlights.Add($"Status: {HighlightMatches(invoice.Status.ToString(), searchTerms)}");
                }

                if (ContainsAnyTerm(invoice.Notes, searchTerms))
                {
                    matchedFields.Add(nameof(invoice.Notes));
                    highlights.Add($"Notes: {HighlightMatches(invoice.Notes, searchTerms)}");
                }

                if (ContainsAnyTerm(invoice.Terms, searchTerms))
                {
                    matchedFields.Add(nameof(invoice.Terms));
                    highlights.Add($"Terms: {HighlightMatches(invoice.Terms, searchTerms)}");
                }

                // Search in amount (convert to string)
                var amountString = invoice.Amount.ToString("C");
                if (ContainsAnyTerm(amountString, searchTerms))
                {
                    matchedFields.Add(nameof(invoice.Amount));
                    highlights.Add($"Amount: {HighlightMatches(amountString, searchTerms)}");
                }

                // Search in related client name
                if (ContainsAnyTerm(invoice.Client?.Name, searchTerms))
                {
                    matchedFields.Add("ClientName");
                    highlights.Add($"Client: {HighlightMatches(invoice.Client.Name, searchTerms)}");
                }

                // Search in related job title
                if (ContainsAnyTerm(invoice.Job?.Title, searchTerms))
                {
                    matchedFields.Add("JobTitle");
                    highlights.Add($"Job: {HighlightMatches(invoice.Job.Title, searchTerms)}");
                }

                if (matchedFields.Count > 0)
                {
                    matchingInvoices.Add((invoice, matchedFields, string.Join(" | ", highlights)));
                }
            }

            results.Invoices = matchingInvoices
                .Take(maxResults)
                .Select(match => new SearchResultItem<InvoiceDto>
                {
                    Item = match.Invoice.Adapt<InvoiceDto>(),
                    EntityType = SearchEntity.Invoices,
                    MatchedFields = match.MatchedFields,
                    Highlight = match.Highlight
                })
                .ToList();
        }

        private static bool ContainsAnyTerm(string? text, string[] searchTerms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return searchTerms.Any(term => 
                text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private static string HighlightMatches(string? text, string[] searchTerms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var result = text;
            foreach (var term in searchTerms)
            {
                var pattern = Regex.Escape(term);
                result = Regex.Replace(result, pattern, $"**{term}**", RegexOptions.IgnoreCase);
            }

            // Truncate if too long
            if (result.Length > 100)
            {
                var firstMatch = result.IndexOf("**");
                if (firstMatch > 0)
                {
                    var start = Math.Max(0, firstMatch - 30);
                    var length = Math.Min(100, result.Length - start);
                    result = "..." + result.Substring(start, length) + "...";
                }
                else
                {
                    result = result.Substring(0, 100) + "...";
                }
            }

            return result;
        }
    }
}