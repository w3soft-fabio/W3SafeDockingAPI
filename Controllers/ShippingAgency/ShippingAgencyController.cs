using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/shippingagencies")]
    [Authorize]
    public class ShippingAgencyController : ControllerBase
    {
        private readonly ShippingAgencyService _service;

        public ShippingAgencyController(ShippingAgencyService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/shippingagencies/getShippingAgencies
        /// Retorna lista paginada de agências marítimas com busca e ordenação.
        /// </summary>
        [HttpGet("getShippingAgencies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<ShippingAgencyResponseDTO>>> GetShippingAgencies(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchShippingAgenciesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/shippingagencies/getShippingAgency/{id}
        /// Retorna uma agência marítima específica pelo ID.
        /// </summary>
        [HttpGet("getShippingAgency/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShippingAgencyResponseDTO>> GetShippingAgency(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da agência marítima deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var agency = await _service.GetByIdAsync(id);
            if (agency == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Agência marítima não encontrada",
                    Detail = $"Nenhuma agência marítima encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(agency);
        }

        /// <summary>
        /// POST: api/shippingagencies/setShippingAgency
        /// Cria uma nova agência marítima.
        /// </summary>
        [HttpPost("setShippingAgency")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShippingAgencyResponseDTO>> SetShippingAgency([FromBody] ShippingAgencyCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados da agência marítima são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novaAgency = await _service.CriarShippingAgencyAsync(dto);

            return CreatedAtAction(
                nameof(GetShippingAgency),
                new { id = novaAgency.AgencyID },
                novaAgency
            );
        }

        /// <summary>
        /// PUT: api/shippingagencies/atualizarShippingAgency/{id}
        /// Atualiza uma agência marítima existente.
        /// </summary>
        [HttpPut("atualizarShippingAgency/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShippingAgencyResponseDTO>> AtualizarShippingAgency(
            int id,
            [FromBody] ShippingAgencyCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados da agência marítima são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var agencyAtualizada = await _service.AtualizarShippingAgencyAsync(id, dto);
            if (agencyAtualizada == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Agência marítima não encontrada",
                    Detail = $"Nenhuma agência marítima encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(agencyAtualizada);
        }

        /// <summary>
        /// DELETE: api/shippingagencies/deletarShippingAgency/{id}
        /// Remove uma agência marítima pelo ID.
        /// </summary>
        [HttpDelete("deletarShippingAgency/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarShippingAgency(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID da agência marítima deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarShippingAgencyAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Agência marítima não encontrada",
                    Detail = $"Nenhuma agência marítima encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
