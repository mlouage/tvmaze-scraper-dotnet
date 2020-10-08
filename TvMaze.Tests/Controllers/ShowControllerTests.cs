using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TvMaze.Models;
using Xunit;

namespace TvMaze.Tests.Controllers
{
    public class ShowControllerTests : IClassFixture<TvMazeWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly TvMazeWebApplicationFactory<Startup> _factory;

        public ShowControllerTests(TvMazeWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Theory]
        [InlineData("/api/shows")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 2)]
        public async Task Get_EndpointsReturnAskedNumberOfItems(int page, int pageSize)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/shows?page={page}&pageSize={pageSize}");
            var result = await response.Content.ReadAsStringAsync();

            var actual = JsonConvert.DeserializeObject<IEnumerable<ShowResponse>>(result);

            Assert.Equal(pageSize, actual.Count());
        }

        [Theory]
        [InlineData(2, 2)]
        public async Task Get_EndpointsReturnRemainingNumberOfItems(int page, int pageSize)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/shows?page={page}&pageSize={pageSize}");
            var result = await response.Content.ReadAsStringAsync();

            var actual = JsonConvert.DeserializeObject<IEnumerable<ShowResponse>>(result);

            Assert.Single(actual);
        }

        [Theory]
        [InlineData(-1, -5)]
        public async Task Get_EndpointsDiscardNegativeInput(int page, int pageSize)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/shows?page={page}&pageSize={pageSize}");
            var result = await response.Content.ReadAsStringAsync();

            var actual = JsonConvert.DeserializeObject<IEnumerable<ShowResponse>>(result);

            Assert.Equal(5, actual.Count());
        }
    }
}
