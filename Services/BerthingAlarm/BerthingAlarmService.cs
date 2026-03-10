using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class BerthingAlarmService
    {
        private readonly IBerthingAlarmRepository _repository;

        public BerthingAlarmService(IBerthingAlarmRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um alarme pelo ID.
        /// </summary>
        public async Task<BerthingAlarmResponseDTO?> GetByIdAsync(int id)
        {
            var alarm = await _repository.GetByIdAsync(id);
            return alarm != null ? BerthingAlarmResponseDTO.FromBerthingAlarm(alarm) : null;
        }

        /// <summary>
        /// Cria um novo alarme de atracação.
        /// </summary>
        public async Task<BerthingAlarmResponseDTO> CriarBerthingAlarmAsync(BerthingAlarmCreateUpdateDTO dto)
        {
            // Converter DTO para entidade
            var novoAlarm = dto.ToEntity();

            // Salvar no banco
            var alarmCriado = await _repository.CreateAsync(novoAlarm);

            return BerthingAlarmResponseDTO.FromBerthingAlarm(alarmCriado);
        }

        /// <summary>
        /// Atualiza um alarme de atracação existente.
        /// </summary>
        public async Task<BerthingAlarmResponseDTO?> AtualizarBerthingAlarmAsync(
            int id,
            BerthingAlarmCreateUpdateDTO dto)
        {
            // Buscar entidade existente
            var alarmExistente = await _repository.GetByIdAsync(id);

            if (alarmExistente == null) return null;

            // Atualizar propriedades
            alarmExistente.MaxShipAngle = dto.MaxShipAngle;
            alarmExistente.ZcMinRange = dto.ZcMinRange;
            alarmExistente.ZcMaxRange = dto.ZcMaxRange;
            alarmExistente.ZcSpeedWarning = dto.ZcSpeedWarning;
            alarmExistente.ZcSpeedAlarm = dto.ZcSpeedAlarm;
            alarmExistente.ZmMinRange = dto.ZmMinRange;
            alarmExistente.ZmMaxRange = dto.ZmMaxRange;
            alarmExistente.ZmSpeedWarning = dto.ZmSpeedWarning;
            alarmExistente.ZmSpeedAlarm = dto.ZmSpeedAlarm;
            alarmExistente.ZfMinRange = dto.ZfMinRange;
            alarmExistente.ZfMaxRange = dto.ZfMaxRange;
            alarmExistente.ZfSpeedWarning = dto.ZfSpeedWarning;
            alarmExistente.ZfSpeedAlarm = dto.ZfSpeedAlarm;

            // Atualizar no banco
            var atualizado = await _repository.UpdateAsync(alarmExistente);

            if (!atualizado) return null;

            // Retornar DTO do alarme atualizado
            return BerthingAlarmResponseDTO.FromBerthingAlarm(alarmExistente);
        }

        /// <summary>
        /// Deleta um alarme pelo ID.
        /// </summary>
        public async Task<bool> DeletarBerthingAlarmAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca alarmes com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<BerthingAlarmResponseDTO>> SearchBerthingAlarmsAsync(
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
