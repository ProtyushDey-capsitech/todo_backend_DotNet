using Projects.Dtos.Todo;
using Projects.Models;

namespace Projects.Dtos.Task
{
    public class TaskLookupProject:TaskModel
    {
        public List<ProjectModel>? project { get; set; }

    }
}
