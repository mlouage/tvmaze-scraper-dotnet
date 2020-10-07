using System;
using System.Collections.Generic;

namespace TvMaze.Entities
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public ICollection<ShowActor> ShowActors { get; set; }
    }
}
