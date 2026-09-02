using Projects.Models;

namespace Projects.Dtos.Project
{
    public class projectTaskunwind
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TaskModel Tasks { get; set; }
    }
}
