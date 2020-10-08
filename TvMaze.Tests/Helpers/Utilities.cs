using System;
using System.Collections.Generic;
using TvMaze.Entities;

namespace TvMaze.Tests.Helpers
{
    public static class Utilities
    {
        public static void InitializeDbForTests(TvMazeContext db)
        {
            var shows = new List<Show>
            {
                new Show {Id = 1, Name = "Show 1"},
                new Show {Id = 2, Name = "Show 2"},
                new Show {Id = 3, Name = "Show 3"},
                new Show {Id = 4, Name = "Show 4"},
                new Show {Id = 5, Name = "Show 5"}
            };

            var actors = new List<Actor>
            {
                new Actor {Id = 1, Name = "Actor 1", DateOfBirth = new DateTime(1990, 5, 5)},
                new Actor {Id = 2, Name = "Actor 2", DateOfBirth = new DateTime(1989, 5, 5)},
                new Actor {Id = 3, Name = "Actor 3", DateOfBirth = new DateTime(1988, 5, 5)},
                new Actor {Id = 4, Name = "Actor 4", DateOfBirth = new DateTime(1987, 5, 5)},
                new Actor {Id = 5, Name = "Actor 5", DateOfBirth = new DateTime(1986, 5, 5)},
                new Actor {Id = 6, Name = "Actor 6", DateOfBirth = new DateTime(1985, 5, 5)}
            };

            var showActors = new List<ShowActor>
            {
                new ShowActor {ShowId = 1, ActorId = 1},
                new ShowActor {ShowId = 1, ActorId = 3},
                new ShowActor {ShowId = 1, ActorId = 6},
                new ShowActor {ShowId = 2, ActorId = 2},
                new ShowActor {ShowId = 2, ActorId = 4},
                new ShowActor {ShowId = 3, ActorId = 5},
                new ShowActor {ShowId = 3, ActorId = 1},
                new ShowActor {ShowId = 3, ActorId = 4},
                new ShowActor {ShowId = 4, ActorId = 6},
                new ShowActor {ShowId = 4, ActorId = 4},
                new ShowActor {ShowId = 5, ActorId = 5},
                new ShowActor {ShowId = 5, ActorId = 3}
            };

            db.Shows.AddRange(shows);
            db.Actors.AddRange(actors);
            db.ShowActor.AddRange(showActors);

            db.SaveChanges();
        }
    }
}
