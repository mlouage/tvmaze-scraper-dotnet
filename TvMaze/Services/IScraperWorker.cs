using System.Collections.Generic;
using System.Threading.Tasks;
using TvMaze.Http;
using TvMaze.Models;

namespace TvMaze.Services
{
    public interface IScraperWorker
    {
        Task<IEnumerable<ShowDto>> GetShowsForPage(int page);

        Task<IEnumerable<ActorDto>> GetActorsForShow(int tvMazeId);
    }
}
