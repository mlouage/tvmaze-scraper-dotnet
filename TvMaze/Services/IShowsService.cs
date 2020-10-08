using System.Collections.Generic;
using TvMaze.Models;

namespace TvMaze.Services
{
    public interface IShowsService
    {
        IEnumerable<ShowResponse> GetShows(int page, int pageSize);
    }
}