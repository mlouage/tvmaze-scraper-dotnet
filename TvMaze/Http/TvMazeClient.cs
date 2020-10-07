using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using TvMaze.Configuration;

namespace TvMaze.Http
{
    public class TvMazeClient
    {
        public HttpClient Client { get; }

        public TvMazeClient(HttpClient client, IOptions<TvMazeOptions> options)
        {
            Client = client;
            Client.BaseAddress = new Uri(options.Value?.Url ?? "https://api.tvmaze.com");
        }
    }
}
