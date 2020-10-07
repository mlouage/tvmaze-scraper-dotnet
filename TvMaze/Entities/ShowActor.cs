namespace TvMaze.Entities
{
    public class ShowActor
    {
        public int ShowId { get; set; }
        public int ActorId { get; set; }
        public Show Show { get; set; }
        public Actor Actor { get; set; }
    }
}
