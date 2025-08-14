using tradetrackr.api.Models;

namespace tradetrackr.api.Dto
{
    public class GlobalSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public SearchEntity[]? EntityTypes { get; set; } // If null, search all entities
        public int MaxResults { get; set; } = 50;
    }

    public class GlobalSearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public SearchResults Results { get; set; } = new();
    }

    public class SearchResults
    {
        public List<SearchResultItem<ClientDto>> Clients { get; set; } = new();
        public List<SearchResultItem<JobDto>> Jobs { get; set; } = new();
        public List<SearchResultItem<InvoiceDto>> Invoices { get; set; } = new();
    }

    public class SearchResultItem<T>
    {
        public T Item { get; set; }
        public SearchEntity EntityType { get; set; }
        public List<string> MatchedFields { get; set; } = new();
        public string Highlight { get; set; } = string.Empty;
    }

    public enum SearchEntity
    {
        Clients,
        Jobs,
        Invoices
    }
}