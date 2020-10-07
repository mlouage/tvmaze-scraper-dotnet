using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TvMaze.Configuration;
using TvMaze.Entities;

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
            _logger.LogInformation("Starting backgroundservice...");

            try
            {
                using (var scope = _services.CreateScope())
                {
                    var worker = scope.ServiceProvider.GetRequiredService<IScraperWorker>();
                    var tvMazeContext = scope.ServiceProvider.GetRequiredService<TvMazeContext>();
                    var tvMazeOptions = scope.ServiceProvider.GetRequiredService<IOptions<TvMazeOptions>>().Value;

                    var page = CalculatePage(tvMazeContext, tvMazeOptions);

                    _logger.LogInformation($"Getting shows for page {page}.");

                    while (true)
                    {
                        var shows = (await worker.GetShowsForPage(page));

                        if (shows == null)
                        {
                            break;
                        }

                        _logger.LogInformation($"Received {shows.Count()} shows");

                        foreach (var show in shows)
                        {
                            var actors = (await worker.GetActorsForShow(show.Id)).ToList();

                            _logger.LogInformation($"Received {actors.Count()} actors for {show.Name}.");

                            var existingActorIds = tvMazeContext.Actors.Select(a => a.Id).ToList();
                            var actorsToAdd = actors.Where(actor => !existingActorIds.Contains(actor.Id))
                                .GroupBy(a => a.Id).Select(a => a.First()).ToList();

                            await tvMazeContext.Actors.AddRangeAsync(actorsToAdd
                                .Select(actor => new Actor
                                {
                                    Id = actor.Id,
                                    Name = actor.Name,
                                    DateOfBirth = actor.BirthDay
                                }), stoppingToken);

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

        private static int CalculatePage(TvMazeContext tvMazeContext, TvMazeOptions tvMazeOptions)
        {
            var page = 0;

            if (!tvMazeContext.Shows.Any()) return page;

            var lastAddedShowId = tvMazeContext.Shows.OrderBy(s => s.Id).Last().Id;
            page = Convert.ToInt32(Math.Round(lastAddedShowId / tvMazeOptions.MaxNumberOfShowsPerPage,
                MidpointRounding.ToZero));

            return page;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping backgroundservice...");

            await base.StopAsync(stoppingToken);
        }
    }
}
