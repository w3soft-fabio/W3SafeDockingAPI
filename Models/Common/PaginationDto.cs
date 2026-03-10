namespace WebSafeDockingAPI.Models.Common
{
    /// <summary>
    /// DTO para receber parâmetros de paginação via query string.
    /// </summary>
    public class PaginationRequest
    {
        /// <summary>
        /// Número da página (começa em 1, padrão: 1).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Quantidade de itens por página (máximo 100, padrão: 10).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Termo de busca opcional.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Campo para ordenação (padrão: "Name").
        /// </summary>
        public string SortBy { get; set; } = "Name";

        /// <summary>
        /// Ordem: "asc" ou "desc" (padrão: "asc").
        /// </summary>
        public string SortOrder { get; set; } = "asc";
    }

    /// <summary>
    /// Resposta paginada genérica estilo Supabase.
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
