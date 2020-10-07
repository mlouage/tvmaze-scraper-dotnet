using Microsoft.EntityFrameworkCore;

namespace TvMaze.Entities
{
    public class TvMazeContext : DbContext
    {
        public DbSet<Show> Shows { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<ShowActor> ShowActor { get; set; }

        public TvMazeContext(DbContextOptions<TvMazeContext> options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShowActor>().HasKey(c => new { c.ShowId, c.ActorId });
        }
    }
}
