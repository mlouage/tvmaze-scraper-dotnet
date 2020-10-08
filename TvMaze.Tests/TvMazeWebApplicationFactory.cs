using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TvMaze.Configuration;
using TvMaze.Entities;
using TvMaze.Services;
using TvMaze.Tests.Helpers;

namespace TvMaze.Tests
{
    public class TvMazeWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup: class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var database = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TvMazeContext>));
                services.Remove(database);

                services.AddDbContext<TvMazeContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var backgroundService = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ScraperService));
                services.Remove(backgroundService);

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<TvMazeContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<TvMazeWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        Utilities.InitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                                            "database with test messages. Error: {Message}", ex.Message);
                    }
                }
            });
        }
    }
}
