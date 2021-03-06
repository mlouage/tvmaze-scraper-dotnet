using Microsoft.AspNetCore.Mvc;
using TvMaze.Services;

namespace TvMaze.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly IShowsService _showsService;

        public ShowsController(IShowsService showsService)
        {
            _showsService = showsService;
        }

        [HttpGet]
        public IActionResult Get(int page = 0, int pageSize = 50)
        {
            if (page < 0)
            {
                page = 0;
            }

            if (pageSize < 0)
            {
                pageSize = 50;
            }

            var shows = _showsService.GetShows(page, pageSize);

            return Ok(shows);
        }
    }
}
