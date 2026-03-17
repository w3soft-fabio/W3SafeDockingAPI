using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/berthings")]
    [Authorize]
    public class BerthingCrudController : ControllerBase
    {
        private readonly BerthingService _service;

        public BerthingCrudController(BerthingService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/berthings/getBerthings
        /// Retorna lista paginada de atracações com busca e ordenação.
        /// </summary>
        [HttpGet("getBerthings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<BerthingResponseDTO>>> GetBerthings(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchBerthingsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/berthings/getBerthing/{id}
        /// Retorna uma atracação específica pelo ID.
        /// </summary>
        [HttpGet("getBerthing/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BerthingResponseDTO>> GetBerthing(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da atracação deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var berthing = await _service.GetByIdAsync(id);
            if (berthing == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Atracação não encontrada",
                    Detail = $"Nenhuma atracação encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(berthing);
        }

        /// <summary>
        /// POST: api/berthings/setBerthing
        /// Cria uma nova atracação.
        /// </summary>
        [HttpPost("setBerthing")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BerthingResponseDTO>> SetBerthing([FromBody] BerthingCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados da atracação são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var novoBerthing = await _service.CriarBerthingAsync(dto);

                return CreatedAtAction(
                    nameof(GetBerthing),
                    new { id = novoBerthing.BerthingID },
                    novoBerthing
                );
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Chave estrangeira inválida",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }

        /// <summary>
        /// PUT: api/berthings/atualizarBerthing/{id}
        /// Atualiza uma atracação existente.
        /// </summary>
        [HttpPut("atualizarBerthing/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BerthingResponseDTO>> AtualizarBerthing(
            int id,
            [FromBody] BerthingCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados da atracação são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var berthingAtualizado = await _service.AtualizarBerthingAsync(id, dto);
                if (berthingAtualizado == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Atracação não encontrada",
                        Detail = $"Nenhuma atracação encontrada com o ID {id}.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(berthingAtualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Chave estrangeira inválida",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }

        /// <summary>
        /// DELETE: api/berthings/deletarBerthing/{id}
        /// Remove uma atracação pelo ID.
        /// </summary>
        [HttpDelete("deletarBerthing/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarBerthing(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da atracação deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarBerthingAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Atracação não encontrada",
                    Detail = $"Nenhuma atracação encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
