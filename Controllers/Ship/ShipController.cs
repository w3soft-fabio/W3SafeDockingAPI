using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/ships")]
    [Authorize]
    public class ShipController : ControllerBase
    {
        private readonly ShipService _service;

        public ShipController(ShipService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/ships/getShips
        /// Retorna lista paginada de navios com busca e ordenação.
        /// </summary>
        [HttpGet("getShips")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<ShipResponseDTO>>> GetShips(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchShipsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/ships/getShip/{id}
        /// Retorna um navio específico pelo ID.
        /// </summary>
        [HttpGet("getShip/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipResponseDTO>> GetShip(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do navio deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var ship = await _service.GetByIdAsync(id);
            if (ship == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Navio não encontrado",
                    Detail = $"Nenhum navio encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(ship);
        }

        /// <summary>
        /// ✅ POST: api/ships/setShip
        /// Cria um novo navio.
        /// </summary>
        [HttpPost("setShip")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShipResponseDTO>> SetShip([FromBody] ShipCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados do navio são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoShip = await _service.CriarShipAsync(dto);

            return CreatedAtAction(
                nameof(GetShip),
                new { id = novoShip.Id },
                novoShip
            );
        }

        /// <summary>
        /// ✏️ PUT: api/ships/atualizarShip/{id}
        /// Atualiza um navio existente.
        /// </summary>
        [HttpPut("atualizarShip/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipResponseDTO>> AtualizarShip(
            int id,
            [FromBody] ShipCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados do navio são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var shipAtualizado = await _service.AtualizarShipAsync(id, dto);
            if (shipAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Navio não encontrado",
                    Detail = $"Nenhum navio encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(shipAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/ships/deletarShip/{id}
        /// Remove um navio pelo ID.
        /// </summary>
        [HttpDelete("deletarShip/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarShip(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do navio deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarShipAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Navio não encontrado",
                    Detail = $"Nenhum navio encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
