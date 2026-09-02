using Projects.Dtos.Task;
using Projects.Models;

namespace Projects.Dtos.Todo
{
    public class ResponseAllTask : ResponseStatusCount
    {
        public List<TaskwithProject>? Tasks { get; set; }
    }
}
