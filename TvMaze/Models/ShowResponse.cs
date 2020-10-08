using System.Collections.Generic;

namespace TvMaze.Models
{
    public class ShowResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<CastResponse> Cast { get; set; }
    }
}
