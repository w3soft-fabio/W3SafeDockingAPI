using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/driftingAlarms")]
    [Authorize]
    public class DriftingAlarmController : ControllerBase
    {
        private readonly DriftingAlarmService _service;

        public DriftingAlarmController(DriftingAlarmService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/driftingAlarms/getDriftingAlarms
        /// Retorna lista paginada de alarmes de deriva com busca e ordenação.
        /// </summary>
        [HttpGet("getDriftingAlarms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<DriftingAlarmResponseDTO>>> GetDriftingAlarms(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchDriftingAlarmsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/driftingAlarms/getDriftingAlarm/{id}
        /// Retorna um alarme de deriva específico pelo ID.
        /// </summary>
        [HttpGet("getDriftingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriftingAlarmResponseDTO>> GetDriftingAlarm(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do alarme deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var alarm = await _service.GetByIdAsync(id);
            if (alarm == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarme não encontrado",
                    Detail = $"Nenhum alarme de deriva encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarm);
        }

        /// <summary>
        /// ✅ POST: api/driftingAlarms/setDriftingAlarm
        /// Cria um novo alarme de deriva.
        /// </summary>
        [HttpPost("setDriftingAlarm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DriftingAlarmResponseDTO>> SetDriftingAlarm(
            [FromBody] DriftingAlarmCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados do alarme são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoAlarm = await _service.CriarDriftingAlarmAsync(dto);

            return CreatedAtAction(
                nameof(GetDriftingAlarm),
                new { id = novoAlarm.Id },
                novoAlarm
            );
        }

        /// <summary>
        /// ✏️ PUT: api/driftingAlarms/atualizarDriftingAlarm/{id}
        /// Atualiza um alarme de deriva existente.
        /// </summary>
        [HttpPut("atualizarDriftingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriftingAlarmResponseDTO>> AtualizarDriftingAlarm(
            int id,
            [FromBody] DriftingAlarmCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser maior que zero e os dados não podem ser nulos.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var alarmAtualizado = await _service.AtualizarDriftingAlarmAsync(id, dto);
            if (alarmAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarme não encontrado",
                    Detail = $"Nenhum alarme de deriva encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarmAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/driftingAlarms/deletarDriftingAlarm/{id}
        /// Remove um alarme de deriva pelo ID.
        /// </summary>
        [HttpDelete("deletarDriftingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarDriftingAlarm(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do alarme deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarDriftingAlarmAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarme não encontrado",
                    Detail = $"Nenhum alarme de deriva encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
