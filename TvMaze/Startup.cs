using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            services.AddPolly();

            services.AddHttpClient<TvMazeClient>()
                .AddPolicyHandlerFromRegistry("Default");

            services.AddControllers();

            services.AddSwaggerGen();
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TvMaze API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
