using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TvMaze.Http;
using TvMaze.Models;

namespace TvMaze.Services
{
    public class ScraperWorker : IScraperWorker
    {
        private readonly TvMazeClient _tvMazeClient;

        public ScraperWorker(TvMazeClient tvMazeClient)
        {
            _tvMazeClient = tvMazeClient;
        }

        public async Task<IEnumerable<ShowDto>> GetShowsForPage(int page)
        {
            var shows = new List<ShowDto>();

            try
            {
                var response = await _tvMazeClient.Client.GetAsync($"/shows?page={page}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                var result = await response.Content.ReadAsStringAsync();
                shows.AddRange(JsonConvert.DeserializeObject<IEnumerable<ShowDto>>(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return shows;
        }

        public async Task<IEnumerable<ActorDto>> GetActorsForShow(int tvMazeId)
        {
            var actors = new List<ActorDto>();

            try
            {
                var response = await _tvMazeClient.Client.GetAsync($"/shows/{tvMazeId}/cast");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                var result = await response.Content.ReadAsStringAsync();
                var persons = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(result);
                actors.AddRange(persons.Select(p => p.Person));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return actors;
        }
    }
}
