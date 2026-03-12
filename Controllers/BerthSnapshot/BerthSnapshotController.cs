using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/berth-snapshots")]
    [Authorize]
    public class BerthSnapshotController : ControllerBase
    {
        private readonly BerthSnapshotService _service;

        public BerthSnapshotController(BerthSnapshotService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/berth-snapshots
        /// Retorna histórico de snapshots com filtros e paginação.
        /// Query params: BercoId, StartDate, EndDate, Page, PageSize, SortBy, SortOrder
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<BerthSnapshotResponseDTO>>> GetSnapshots(
            [FromQuery] BerthSnapshotSearchRequest request)
        {
            var result = await _service.SearchAsync(request);
            return Ok(result);
        }
    }
}
