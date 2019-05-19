namespace AnimalAi.Data
{
    public class Animal
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual Question Parent { get; set; }

        public virtual bool Answer { get; set; }
    }
}