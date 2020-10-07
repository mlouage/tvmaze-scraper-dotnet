using System.Collections.Generic;

namespace TvMaze.Entities
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ShowActor> ShowActors { get; set; }
    }
}
