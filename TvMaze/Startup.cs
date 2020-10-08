using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using TvMaze.Configuration;
using TvMaze.Entities;
using TvMaze.Http;
using TvMaze.Services;

namespace TvMaze
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TvMazeContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()

            );

            services.Configure<TvMazeOptions>(Configuration.GetSection("TvMazeOptions"));

            services.AddTransient<IScraperWorker, ScraperWorker>();
            services.AddTransient<IShowsService, ShowsService>();

            services.AddHostedService<ScraperService>();

            var registry = services.AddPolicyRegistry();

            var defaultPolicy = Policy
                .HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.TooManyRequests)
                .OrTransientHttpError()
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            registry.Add("Default", defaultPolicy);

            services.AddHttpClient<TvMazeClient>()
                .AddPolicyHandlerFromRegistry("Default");

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TvMazeContext tvMazeContext)
        {
            try
            {
                tvMazeContext.Database.Migrate();
                Console.WriteLine("Done migrating database.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
