using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/mooringpatterns")]
    [Authorize]
    public class MooringPatternController : ControllerBase
    {
        private readonly MooringPatternService _service;

        public MooringPatternController(MooringPatternService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/mooringpatterns/getMooringPatterns
        /// Retorna lista paginada de mooring patterns com busca e ordenação.
        /// </summary>
        [HttpGet("getMooringPatterns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<MooringPatternResponseDTO>>> GetMooringPatterns(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchMooringPatternsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/mooringpatterns/getMooringPattern/{id}
        /// Retorna um mooring pattern específico pelo ID.
        /// </summary>
        [HttpGet("getMooringPattern/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MooringPatternResponseDTO>> GetMooringPattern(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do mooring pattern deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var mp = await _service.GetByIdAsync(id);
            if (mp == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Mooring pattern não encontrado",
                    Detail = $"Nenhum mooring pattern encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(mp);
        }

        /// <summary>
        /// ✅ POST: api/mooringpatterns/setMooringPattern
        /// Cria um novo mooring pattern.
        /// </summary>
        [HttpPost("setMooringPattern")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MooringPatternResponseDTO>> SetMooringPattern(
            [FromBody] MooringPatternCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados do mooring pattern são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoMp = await _service.CriarMooringPatternAsync(dto);

            return CreatedAtAction(
                nameof(GetMooringPattern),
                new { id = novoMp.Id },
                novoMp
            );
        }

        /// <summary>
        /// ✏️ PUT: api/mooringpatterns/atualizarMooringPattern/{id}
        /// Atualiza um mooring pattern existente.
        /// </summary>
        [HttpPut("atualizarMooringPattern/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MooringPatternResponseDTO>> AtualizarMooringPattern(
            long id,
            [FromBody] MooringPatternCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados do mooring pattern são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var mpAtualizado = await _service.AtualizarMooringPatternAsync(id, dto);
            if (mpAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Mooring pattern não encontrado",
                    Detail = $"Nenhum mooring pattern encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(mpAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/mooringpatterns/deletarMooringPattern/{id}
        /// Deleta um mooring pattern pelo ID.
        /// </summary>
        [HttpDelete("deletarMooringPattern/{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarMooringPattern(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarMooringPatternAsync(id);
            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Mooring pattern não encontrado",
                    Detail = $"Nenhum mooring pattern encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
