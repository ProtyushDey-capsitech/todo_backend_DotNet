using Projects.Models;

namespace Projects.Dtos.Task
{
    public class TaskUnwindProject : TaskModel
    {
        public ProjectModel? project { get; set; }
    }
}
