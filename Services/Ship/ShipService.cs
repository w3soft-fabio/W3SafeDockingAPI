using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class ShipService
    {
        private readonly IShipRepository _repository;

        public ShipService(IShipRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um navio pelo ID.
        /// </summary>
        public async Task<ShipResponseDTO?> GetByIdAsync(int id)
        {
            var ship = await _repository.GetByIdAsync(id);
            return ship != null ? ShipResponseDTO.FromShip(ship) : null;
        }

        /// <summary>
        /// Cria um novo navio.
        /// </summary>
        public async Task<ShipResponseDTO> CriarShipAsync(ShipCreateUpdateDTO dto)
        {
            // Converter DTO para entidade
            var novoShip = dto.ToEntity();

            // Salvar no banco
            var shipCriado = await _repository.CreateAsync(novoShip);

            return ShipResponseDTO.FromShip(shipCriado);
        }

        /// <summary>
        /// Atualiza um navio existente.
        /// </summary>
        public async Task<ShipResponseDTO?> AtualizarShipAsync(int id, ShipCreateUpdateDTO dto)
        {
            // Buscar entidade existente
            var shipExistente = await _repository.GetByIdAsync(id);

            if (shipExistente == null) return null;

            // Atualizar propriedades
            shipExistente.Imo = dto.Imo;
            shipExistente.Name = dto.Name;
            shipExistente.Length = dto.Length;
            shipExistente.Width = dto.Width;
            shipExistente.Dwt = dto.Dwt;
            shipExistente.AlarmID = dto.AlarmID;

            // Atualizar no banco
            var atualizado = await _repository.UpdateAsync(shipExistente);

            if (!atualizado) return null;

            // Retornar DTO do navio atualizado
            return ShipResponseDTO.FromShip(shipExistente);
        }

        /// <summary>
        /// Deleta um navio pelo ID.
        /// </summary>
        public async Task<bool> DeletarShipAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca navios por termo de busca com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<ShipResponseDTO>> SearchShipsAsync(
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }
    }
}
