using System.Collections.Generic;

namespace AnimalAi.Data
{
    public class Question
    {
        public virtual int Id { get; set; }

        public virtual string Data { get; set; }

        public virtual Question Parent { get; set; }

        public virtual bool? Answer { get; set; }

        public virtual IList<Question> Children { get; set; } = new List<Question>();
    }
}