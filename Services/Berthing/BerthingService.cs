using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class BerthingService
    {
        private readonly IBerthingRepository _repository;
        private readonly IShipRepository _shipRepository;
        private readonly IMooringCompanyRepository _mooringCompanyRepository;
        private readonly IShippingAgencyRepository _shippingAgencyRepository;

        public BerthingService(
            IBerthingRepository repository,
            IShipRepository shipRepository,
            IMooringCompanyRepository mooringCompanyRepository,
            IShippingAgencyRepository shippingAgencyRepository)
        {
            _repository = repository;
            _shipRepository = shipRepository;
            _mooringCompanyRepository = mooringCompanyRepository;
            _shippingAgencyRepository = shippingAgencyRepository;
        }

        public async Task<BerthingResponseDTO?> GetByIdAsync(int id)
        {
            var berthing = await _repository.GetByIdAsync(id);
            return berthing != null ? BerthingResponseDTO.FromBerthing(berthing) : null;
        }

        public async Task<BerthingResponseDTO> CriarBerthingAsync(BerthingCreateUpdateDTO dto)
        {
            await ValidarChavesEstrangeirasAsync(dto);

            var novoBerthing = dto.ToEntity();
            var berthingCriado = await _repository.CreateAsync(novoBerthing);
            return BerthingResponseDTO.FromBerthing(berthingCriado);
        }

        public async Task<BerthingResponseDTO?> AtualizarBerthingAsync(int id, BerthingCreateUpdateDTO dto)
        {
            var berthingExistente = await _repository.GetByIdAsync(id);
            if (berthingExistente == null) return null;

            await ValidarChavesEstrangeirasAsync(dto);

            berthingExistente.Berth = dto.Berth;
            berthingExistente.Schedule = dto.Schedule;
            berthingExistente.ShipID = dto.ShipID;
            berthingExistente.MooringCompanyID = dto.MooringCompanyID;
            berthingExistente.AgencyID = dto.AgencyID;
            berthingExistente.ArrivalDraftFore = dto.ArrivalDraftFore;
            berthingExistente.ArrivalDraftAft = dto.ArrivalDraftAft;
            berthingExistente.DepartureDraftFore = dto.DepartureDraftFore;
            berthingExistente.DepartureDraftAft = dto.DepartureDraftAft;
            berthingExistente.UnberthingDate = dto.UnberthingDate;
            berthingExistente.Side = dto.Side;
            berthingExistente.ArrivalAt = dto.ArrivalAt ?? berthingExistente.ArrivalAt;
            berthingExistente.DepartureAt = dto.DepartureAt ?? berthingExistente.DepartureAt;

            var atualizado = await _repository.UpdateAsync(berthingExistente);
            if (!atualizado) return null;

            return BerthingResponseDTO.FromBerthing(berthingExistente);
        }

        public async Task<bool> DeletarBerthingAsync(int id) =>
            await _repository.DeleteAsync(id);

        public async Task<PaginatedResponse<BerthingResponseDTO>> SearchBerthingsAsync(
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

        private async Task ValidarChavesEstrangeirasAsync(BerthingCreateUpdateDTO dto)
        {
            var ship = await _shipRepository.GetByIdAsync(dto.ShipID);
            if (ship == null)
                throw new KeyNotFoundException($"Navio com ID {dto.ShipID} não encontrado.");

            if (dto.MooringCompanyID.HasValue)
            {
                var mooringCompany = await _mooringCompanyRepository.GetByIdAsync(dto.MooringCompanyID.Value);
                if (mooringCompany == null)
                    throw new KeyNotFoundException($"Empresa de amarração com ID {dto.MooringCompanyID.Value} não encontrada.");
            }

            if (dto.AgencyID.HasValue)
            {
                var agency = await _shippingAgencyRepository.GetByIdAsync(dto.AgencyID.Value);
                if (agency == null)
                    throw new KeyNotFoundException($"Agência marítima com ID {dto.AgencyID.Value} não encontrada.");
            }
        }
    }
}
