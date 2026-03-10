using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/berthingAlarms")]
    [Authorize]
    public class BerthingAlarmController : ControllerBase
    {
        private readonly BerthingAlarmService _service;

        public BerthingAlarmController(BerthingAlarmService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/berthingAlarms/getBerthingAlarms
        /// Retorna lista paginada de alarmes com busca e ordenação.
        /// </summary>
        [HttpGet("getBerthingAlarms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<BerthingAlarmResponseDTO>>> GetBerthingAlarms(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchBerthingAlarmsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/berthingAlarms/getBerthingAlarm/{id}
        /// Retorna um alarme específico pelo ID.
        /// </summary>
        [HttpGet("getBerthingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BerthingAlarmResponseDTO>> GetBerthingAlarm(int id)
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
                    Detail = $"Nenhum alarme encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarm);
        }

        /// <summary>
        /// ✅ POST: api/berthingAlarms/setBerthingAlarm
        /// Cria um novo alarme de atracação.
        /// </summary>
        [HttpPost("setBerthingAlarm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BerthingAlarmResponseDTO>> SetBerthingAlarm(
            [FromBody] BerthingAlarmCreateUpdateDTO dto)
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

            var novoAlarm = await _service.CriarBerthingAlarmAsync(dto);

            return CreatedAtAction(
                nameof(GetBerthingAlarm),
                new { id = novoAlarm.Id },
                novoAlarm
            );
        }

        /// <summary>
        /// ✏️ PUT: api/berthingAlarms/atualizarBerthingAlarm/{id}
        /// Atualiza um alarme de atracação existente.
        /// </summary>
        [HttpPut("atualizarBerthingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BerthingAlarmResponseDTO>> AtualizarBerthingAlarm(
            int id,
            [FromBody] BerthingAlarmCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados do alarme são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var alarmAtualizado = await _service.AtualizarBerthingAlarmAsync(id, dto);
            if (alarmAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarme não encontrado",
                    Detail = $"Nenhum alarme encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarmAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/berthingAlarms/deletarBerthingAlarm/{id}
        /// Remove um alarme pelo ID.
        /// </summary>
        [HttpDelete("deletarBerthingAlarm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarBerthingAlarm(int id)
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

            var sucesso = await _service.DeletarBerthingAlarmAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarme não encontrado",
                    Detail = $"Nenhum alarme encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
