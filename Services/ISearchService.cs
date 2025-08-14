using tradetrackr.api.Dto;
using tradetrackr.api.Models;

namespace tradetrackr.api.Services
{
    public interface ISearchService
    {
        Task<GlobalSearchResponse> SearchAsync(GlobalSearchRequest request, string userId);
    }
}