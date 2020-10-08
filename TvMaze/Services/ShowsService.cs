using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TvMaze.Entities;
using TvMaze.Models;

namespace TvMaze.Services
{
    public class ShowsService : IShowsService
    {
        private readonly TvMazeContext _tvMazeContext;

        public ShowsService(TvMazeContext tvMazeContext)
        {
            _tvMazeContext = tvMazeContext;
        }

        public IEnumerable<ShowResponse> GetShows(int page = 0, int pageSize = 50)
        {
            var shows = _tvMazeContext.Shows
                .Include(s => s.ShowActors)
                .ThenInclude(sp => sp.Actor)
                .OrderBy(s => s.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(show => new ShowResponse
                {
                    Id = show.Id,
                    Name = show.Name,
                    Cast = show.ShowActors
                        .Select(c => c.Actor)
                        .OrderByDescending(a => a.DateOfBirth)
                        .Select(a =>
                            new CastResponse
                            {
                                Id = a.Id,
                                Name = a.Name,
                                BirthDay = a.DateOfBirth
                            })
                });

            return shows;
        }
    }
}
