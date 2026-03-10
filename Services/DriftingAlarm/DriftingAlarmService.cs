using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class DriftingAlarmService
    {
        private readonly IDriftingAlarmRepository _repository;

        public DriftingAlarmService(IDriftingAlarmRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um alarme de deriva pelo ID.
        /// </summary>
        public async Task<DriftingAlarmResponseDTO?> GetByIdAsync(int id)
        {
            var alarm = await _repository.GetByIdAsync(id);
            return alarm != null ? DriftingAlarmResponseDTO.FromDriftingAlarm(alarm) : null;
        }

        /// <summary>
        /// Cria um novo alarme de deriva.
        /// </summary>
        public async Task<DriftingAlarmResponseDTO> CriarDriftingAlarmAsync(DriftingAlarmCreateUpdateDTO dto)
        {
            var novoAlarm = dto.ToEntity();
            var alarmCriado = await _repository.CreateAsync(novoAlarm);
            return DriftingAlarmResponseDTO.FromDriftingAlarm(alarmCriado);
        }

        /// <summary>
        /// Atualiza um alarme de deriva existente.
        /// </summary>
        public async Task<DriftingAlarmResponseDTO?> AtualizarDriftingAlarmAsync(
            int id,
            DriftingAlarmCreateUpdateDTO dto)
        {
            var alarmExistente = await _repository.GetByIdAsync(id);

            if (alarmExistente == null) return null;

            alarmExistente.MaxShipAngle = dto.MaxShipAngle;
            alarmExistente.OutboundWarning = dto.OutboundWarning;
            alarmExistente.OutboundAlarm = dto.OutboundAlarm;
            alarmExistente.InboundWarning = dto.InboundWarning;
            alarmExistente.InboundAlarm = dto.InboundAlarm;

            var atualizado = await _repository.UpdateAsync(alarmExistente);

            if (!atualizado) return null;

            return DriftingAlarmResponseDTO.FromDriftingAlarm(alarmExistente);
        }

        /// <summary>
        /// Deleta um alarme de deriva pelo ID.
        /// </summary>
        public async Task<bool> DeletarDriftingAlarmAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca alarmes de deriva com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<DriftingAlarmResponseDTO>> SearchDriftingAlarmsAsync(
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
