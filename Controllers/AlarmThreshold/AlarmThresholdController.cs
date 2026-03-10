using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/alarmThresholds")]
    [Authorize]
    public class AlarmThresholdController : ControllerBase
    {
        private readonly AlarmThresholdService _service;

        public AlarmThresholdController(AlarmThresholdService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/alarmThresholds/getAlarmThresholds
        /// Retorna lista paginada de alarm thresholds com busca e ordenação.
        /// </summary>
        [HttpGet("getAlarmThresholds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<AlarmThresholdResponseDTO>>> GetAlarmThresholds(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchAlarmThresholdsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/alarmThresholds/getAlarmThreshold/{id}
        /// Retorna um alarm threshold específico pelo ID.
        /// </summary>
        [HttpGet("getAlarmThreshold/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlarmThresholdResponseDTO>> GetAlarmThreshold(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do alarm threshold deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var alarmThreshold = await _service.GetByIdAsync(id);
            if (alarmThreshold == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarm Threshold não encontrado",
                    Detail = $"Nenhum alarm threshold encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarmThreshold);
        }

        /// <summary>
        /// ✅ POST: api/alarmThresholds/setAlarmThreshold
        /// Cria um novo alarm threshold.
        /// </summary>
        [HttpPost("setAlarmThreshold")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AlarmThresholdResponseDTO>> SetAlarmThreshold([FromBody] AlarmThresholdCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados do alarm threshold são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoAlarmThreshold = await _service.CriarAlarmThresholdAsync(dto);

            return CreatedAtAction(
                nameof(GetAlarmThreshold),
                new { id = novoAlarmThreshold.Id },
                novoAlarmThreshold
            );
        }

        /// <summary>
        /// ✏️ PUT: api/alarmThresholds/atualizarAlarmThreshold/{id}
        /// Atualiza um alarm threshold existente.
        /// </summary>
        [HttpPut("atualizarAlarmThreshold/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlarmThresholdResponseDTO>> AtualizarAlarmThreshold(
            int id,
            [FromBody] AlarmThresholdCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados do alarm threshold são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var alarmThresholdAtualizado = await _service.AtualizarAlarmThresholdAsync(id, dto);
            if (alarmThresholdAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarm Threshold não encontrado",
                    Detail = $"Nenhum alarm threshold encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(alarmThresholdAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/alarmThresholds/deletarAlarmThreshold/{id}
        /// Remove um alarm threshold pelo ID.
        /// </summary>
        [HttpDelete("deletarAlarmThreshold/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarAlarmThreshold(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do alarm threshold deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarAlarmThresholdAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Alarm Threshold não encontrado",
                    Detail = $"Nenhum alarm threshold encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
