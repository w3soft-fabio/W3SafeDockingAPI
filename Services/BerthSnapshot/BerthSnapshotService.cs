using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class BerthSnapshotService
    {
        private readonly IBerthSnapshotRepository _repository;

        public BerthSnapshotService(IBerthSnapshotRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Salva os dados dos dois berços de um ModbusSnapshot no banco.
        /// </summary>
        public async Task SaveSnapshotAsync(ModbusSnapshot snapshot)
        {
            var capturedAt = snapshot.DataHoraLeitura;

            var entities = new[]
            {
                BerthSnapshot.FromBerthData(snapshot.Berco1, capturedAt),
                BerthSnapshot.FromBerthData(snapshot.Berco2, capturedAt),
            };

            await _repository.CreateManyAsync(entities);
        }

        /// <summary>
        /// Busca snapshots com filtros e paginação.
        /// </summary>
        public async Task<PaginatedResponse<BerthSnapshotResponseDTO>> SearchAsync(
            BerthSnapshotSearchRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                request.Page,
                request.PageSize,
                request.BercoId,
                request.StartDate,
                request.EndDate,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }
    }
}
