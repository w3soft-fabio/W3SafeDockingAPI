using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/mooringcompanies")]
    [Authorize]
    public class MooringCompanyController : ControllerBase
    {
        private readonly MooringCompanyService _service;

        public MooringCompanyController(MooringCompanyService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/mooringcompanies/getMooringCompanies
        /// Retorna lista paginada de empresas de amarração com busca e ordenação.
        /// </summary>
        [HttpGet("getMooringCompanies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<MooringCompanyResponseDTO>>> GetMooringCompanies(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchMooringCompaniesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/mooringcompanies/getMooringCompany/{id}
        /// Retorna uma empresa de amarração específica pelo ID.
        /// </summary>
        [HttpGet("getMooringCompany/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MooringCompanyResponseDTO>> GetMooringCompany(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da empresa de amarração deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var mooringCompany = await _service.GetByIdAsync(id);
            if (mooringCompany == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Empresa de amarração não encontrada",
                    Detail = $"Nenhuma empresa de amarração encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(mooringCompany);
        }

        /// <summary>
        /// POST: api/mooringcompanies/setMooringCompany
        /// Cria uma nova empresa de amarração.
        /// </summary>
        [HttpPost("setMooringCompany")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MooringCompanyResponseDTO>> SetMooringCompany([FromBody] MooringCompanyCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados da empresa de amarração são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoMooringCompany = await _service.CriarMooringCompanyAsync(dto);

            return CreatedAtAction(
                nameof(GetMooringCompany),
                new { id = novoMooringCompany.MooringCompanyID },
                novoMooringCompany
            );
        }

        /// <summary>
        /// PUT: api/mooringcompanies/atualizarMooringCompany/{id}
        /// Atualiza uma empresa de amarração existente.
        /// </summary>
        [HttpPut("atualizarMooringCompany/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MooringCompanyResponseDTO>> AtualizarMooringCompany(
            int id,
            [FromBody] MooringCompanyCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados da empresa de amarração são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var mooringCompanyAtualizado = await _service.AtualizarMooringCompanyAsync(id, dto);
            if (mooringCompanyAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Empresa de amarração não encontrada",
                    Detail = $"Nenhuma empresa de amarração encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(mooringCompanyAtualizado);
        }

        /// <summary>
        /// DELETE: api/mooringcompanies/deletarMooringCompany/{id}
        /// Remove uma empresa de amarração pelo ID.
        /// </summary>
        [HttpDelete("deletarMooringCompany/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarMooringCompany(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da empresa de amarração deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarMooringCompanyAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Empresa de amarração não encontrada",
                    Detail = $"Nenhuma empresa de amarração encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
