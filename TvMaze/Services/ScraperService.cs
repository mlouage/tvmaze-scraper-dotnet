using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TvMaze.Configuration;
using TvMaze.Entities;
using TvMaze.Models;

namespace TvMaze.Services
{
    public class ScraperService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ScraperService> _logger;

        public ScraperService(IServiceProvider services, ILogger<ScraperService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var worker = scope.ServiceProvider.GetRequiredService<IScraperWorker>();
                    var tvMazeContext = scope.ServiceProvider.GetRequiredService<TvMazeContext>();
                    var tvMazeOptions = scope.ServiceProvider.GetRequiredService<IOptions<TvMazeOptions>>().Value;

                    await WaitForDatabase(stoppingToken, tvMazeContext);

                    var page = CalculatePage(tvMazeContext, tvMazeOptions);

                    while (page < 3) // should be while (true) but the TvMaze is so huge I cut it off locally at 3 pages
                    {
                        var shows = await worker.GetShowsForPage(page);

                        if (shows == null)
                        {
                            break;
                        }

                        foreach (var show in shows)
                        {
                            if (await CheckIfShowIsAlreadySaved(stoppingToken, tvMazeContext, show)) continue;

                            var actors = (await worker.GetActorsForShow(show.Id)).ToList();

                            var actorsToAdd = GetNewActorsToAdd(tvMazeContext, actors);

                            await AddActors(stoppingToken, tvMazeContext, actorsToAdd);

                            await AddShow(stoppingToken, tvMazeContext, show, actorsToAdd);

                            try
                            {
                                await tvMazeContext.SaveChangesAsync(stoppingToken);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }

                        ++page;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);

                Console.WriteLine(e);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping backgroundservice...");

            await base.StopAsync(stoppingToken);
        }

        private static async Task AddShow(CancellationToken stoppingToken, TvMazeContext tvMazeContext, ShowDto show,
            IEnumerable<ActorDto> actorsToAdd)
        {
            await tvMazeContext.Shows.AddAsync(new Show
            {
                Id = show.Id,
                Name = show.Name,
                ShowActors = actorsToAdd
                    .Select(actor => new ShowActor
                    {
                        ShowId = show.Id, ActorId = actor.Id
                    }).ToList()
            }, stoppingToken);
        }

        private static async Task AddActors(CancellationToken stoppingToken, TvMazeContext tvMazeContext,
            IEnumerable<ActorDto> actorsToAdd)
        {
            await tvMazeContext.Actors.AddRangeAsync(actorsToAdd
                .Select(actor => new Actor
                {
                    Id = actor.Id,
                    Name = actor.Name,
                    DateOfBirth = actor.BirthDay
                }), stoppingToken);
        }

        private async Task<bool> CheckIfShowIsAlreadySaved(CancellationToken stoppingToken, TvMazeContext tvMazeContext,
            ShowDto show)
        {
            if (await tvMazeContext.Shows.AnyAsync(s => s.Id == show.Id, stoppingToken))
            {
                _logger.LogInformation($"Show {show.Id} - {show.Name} was already fetched.");
                return true;
            }

            return false;
        }

        private static List<ActorDto> GetNewActorsToAdd(TvMazeContext tvMazeContext, IEnumerable<ActorDto> actors)
        {
            var existingActorIds = tvMazeContext.Actors.Select(a => a.Id).ToList();
            var actorsToAdd = actors.Where(actor => !existingActorIds.Contains(actor.Id))
                .GroupBy(a => a.Id).Select(a => a.First()).ToList();
            return actorsToAdd;
        }

        private async Task WaitForDatabase(CancellationToken stoppingToken, TvMazeContext tvMazeContext)
        {
            while (true)
            {
                if (await tvMazeContext.Database.CanConnectAsync(stoppingToken))
                {
                    break;
                }

                _logger.LogInformation("Database not ready yet...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        private static int CalculatePage(TvMazeContext tvMazeContext, TvMazeOptions tvMazeOptions)
        {
            var page = 0;

            if (!tvMazeContext.Shows.Any()) return page;

            var lastAddedShowId = tvMazeContext.Shows.OrderBy(s => s.Id).Last().Id;
            page = Convert.ToInt32(Math.Round(lastAddedShowId / tvMazeOptions.MaxNumberOfShowsPerPage,
                MidpointRounding.ToZero));

            return page;
        }
    }
}
