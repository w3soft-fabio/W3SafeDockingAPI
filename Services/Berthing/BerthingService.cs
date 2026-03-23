using QuestPDF.Fluent;
using WebSafeDockingAPI.Exceptions;
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
        private readonly IWebHostEnvironment _env;

        public BerthingService(
            IBerthingRepository repository,
            IShipRepository shipRepository,
            IMooringCompanyRepository mooringCompanyRepository,
            IShippingAgencyRepository shippingAgencyRepository,
            IWebHostEnvironment env)
        {
            _repository = repository;
            _shipRepository = shipRepository;
            _mooringCompanyRepository = mooringCompanyRepository;
            _shippingAgencyRepository = shippingAgencyRepository;
            _env = env;
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
            berthingExistente.ShipID = dto.Ship.Id;
            berthingExistente.MooringCompanyID = dto.MooringCompany?.MooringCompanyID;
            berthingExistente.AgencyID = dto.ShippingAgency?.AgencyID;
            berthingExistente.ArrivalDraftFore = dto.ArrivalDraftFore;
            berthingExistente.ArrivalDraftAft = dto.ArrivalDraftAft;
            berthingExistente.DepartureDraftFore = dto.DepartureDraftFore;
            berthingExistente.DepartureDraftAft = dto.DepartureDraftAft;
            berthingExistente.Side = dto.Side;
            berthingExistente.ArrivalAt = dto.ArrivalAt ?? berthingExistente.ArrivalAt;
            berthingExistente.DepartureAt = dto.DepartureAt ?? berthingExistente.DepartureAt;

            var atualizado = await _repository.UpdateAsync(berthingExistente);
            if (!atualizado) return null;

            return BerthingResponseDTO.FromBerthing(berthingExistente);
        }

        public async Task<bool> DeletarBerthingAsync(int id) =>
            await _repository.DeleteAsync(id);

        public async Task<string> GerarRelatorioPdfAsync(
            DateTime dataInicial,
            DateTime dataFinal,
            int? shipID,
            HttpRequest request)
        {
            var berthings = await _repository.GetBerthingsForReportAsync(dataInicial, dataFinal, shipID);

            if (berthings.Count == 0)
                throw new NotFoundException("Nenhuma atracação encontrada no período informado.");

            var document = new BerthingReportDocument(berthings, dataInicial, dataFinal);
            var pdfBytes = document.GeneratePdf();

            var tempDir = Path.Combine(_env.ContentRootPath, "reportsTemp");
            Directory.CreateDirectory(tempDir);

            var fileName = $"{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(tempDir, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            //var url = $"{request.Scheme}://{request.Host}/reportsTemp/{fileName}"; //debug
            var url = $"https://w3soft3.com.br/W3SafeDockingAPI/reportsTemp/{fileName}"; //prod
            return url;
        }

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
            var ship = await _shipRepository.GetByIdAsync(dto.Ship.Id);
            if (ship == null)
                throw new KeyNotFoundException($"Navio com ID {dto.Ship.Id} não encontrado.");

            if (dto.MooringCompany != null)
            {
                var mooringCompany = await _mooringCompanyRepository.GetByIdAsync(dto.MooringCompany.MooringCompanyID);
                if (mooringCompany == null)
                    throw new KeyNotFoundException($"Empresa de amarração com ID {dto.MooringCompany.MooringCompanyID} não encontrada.");
            }

            if (dto.ShippingAgency != null)
            {
                var agency = await _shippingAgencyRepository.GetByIdAsync(dto.ShippingAgency.AgencyID);
                if (agency == null)
                    throw new KeyNotFoundException($"Agência marítima com ID {dto.ShippingAgency.AgencyID} não encontrada.");
            }
        }
    }
}
