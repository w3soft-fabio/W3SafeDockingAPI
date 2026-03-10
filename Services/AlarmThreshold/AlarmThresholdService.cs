using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class AlarmThresholdService
    {
        private readonly IAlarmThresholdRepository _repository;
        private readonly IBerthingAlarmRepository _berthingAlarmRepository;
        private readonly IDriftingAlarmRepository _driftingAlarmRepository;

        public AlarmThresholdService(
            IAlarmThresholdRepository repository,
            IBerthingAlarmRepository berthingAlarmRepository,
            IDriftingAlarmRepository driftingAlarmRepository)
        {
            _repository = repository;
            _berthingAlarmRepository = berthingAlarmRepository;
            _driftingAlarmRepository = driftingAlarmRepository;
        }

        /// <summary>
        /// Busca um alarm threshold pelo ID.
        /// </summary>
        public async Task<AlarmThresholdResponseDTO?> GetByIdAsync(int id)
        {
            var alarmThreshold = await _repository.GetByIdAsync(id);
            return alarmThreshold != null ? AlarmThresholdResponseDTO.FromAlarmThreshold(alarmThreshold) : null;
        }

        /// <summary>
        /// Cria um novo alarm threshold.
        /// </summary>
        public async Task<AlarmThresholdResponseDTO> CriarAlarmThresholdAsync(AlarmThresholdCreateUpdateDTO dto)
        {
            // Validar chave estrangeira BerthingAlarm
            if (dto.BerthingID.HasValue)
            {
                var berthingAlarm = await _berthingAlarmRepository.GetByIdAsync(dto.BerthingID.Value);
                if (berthingAlarm == null)
                    throw new Exception($"BerthingAlarm com ID {dto.BerthingID.Value} não encontrado.");
            }

            // Validar chave estrangeira DriftingAlarm
            if (dto.DriftingID.HasValue)
            {
                var driftingAlarm = await _driftingAlarmRepository.GetByIdAsync(dto.DriftingID.Value);
                if (driftingAlarm == null)
                    throw new Exception($"DriftingAlarm com ID {dto.DriftingID.Value} não encontrado.");
            }

            // Converter DTO para entidade
            var novoAlarmThreshold = dto.ToEntity();

            // Salvar no banco
            var alarmThresholdCriado = await _repository.CreateAsync(novoAlarmThreshold);

            // Buscar novamente com Include para trazer navegação
            var resultado = await _repository.GetByIdAsync(alarmThresholdCriado.Id);

            return AlarmThresholdResponseDTO.FromAlarmThreshold(resultado!);
        }

        /// <summary>
        /// Atualiza um alarm threshold existente.
        /// </summary>
        public async Task<AlarmThresholdResponseDTO?> AtualizarAlarmThresholdAsync(int id, AlarmThresholdCreateUpdateDTO dto)
        {
            // Buscar entidade existente
            var alarmThresholdExistente = await _repository.GetByIdAsync(id);

            if (alarmThresholdExistente == null) return null;

            // Validar chave estrangeira BerthingAlarm
            if (dto.BerthingID.HasValue)
            {
                var berthingAlarm = await _berthingAlarmRepository.GetByIdAsync(dto.BerthingID.Value);
                if (berthingAlarm == null)
                    throw new Exception($"BerthingAlarm com ID {dto.BerthingID.Value} não encontrado.");
            }

            // Validar chave estrangeira DriftingAlarm
            if (dto.DriftingID.HasValue)
            {
                var driftingAlarm = await _driftingAlarmRepository.GetByIdAsync(dto.DriftingID.Value);
                if (driftingAlarm == null)
                    throw new Exception($"DriftingAlarm com ID {dto.DriftingID.Value} não encontrado.");
            }

            // Atualizar propriedades
            alarmThresholdExistente.Name = dto.Name;
            alarmThresholdExistente.BerthingID = dto.BerthingID;
            alarmThresholdExistente.DriftingID = dto.DriftingID;
            alarmThresholdExistente.MinDwt = dto.MinDwt;
            alarmThresholdExistente.MaxDwt = dto.MaxDwt;

            // Atualizar no banco
            var atualizado = await _repository.UpdateAsync(alarmThresholdExistente);

            if (!atualizado) return null;

            // Buscar novamente com Include para trazer navegação atualizada
            var resultado = await _repository.GetByIdAsync(id);

            return AlarmThresholdResponseDTO.FromAlarmThreshold(resultado!);
        }

        /// <summary>
        /// Deleta um alarm threshold pelo ID.
        /// </summary>
        public async Task<bool> DeletarAlarmThresholdAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca alarm thresholds por termo de busca com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<AlarmThresholdResponseDTO>> SearchAlarmThresholdsAsync(
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
