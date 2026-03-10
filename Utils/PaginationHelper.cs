using WebSafeDockingAPI.Models.Common;

namespace WebSafeDockingAPI.Utils
{
    /// <summary>
    /// Helper para criar respostas paginadas de forma consistente.
    /// </summary>
    public static class PaginationHelper
    {
        public static PaginatedResponse<T> CreateResponse<T>(
            List<T> items,
            int totalCount,
            PaginationRequest request)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new PaginatedResponse<T>
            {
                Data = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };
        }
    }
}
