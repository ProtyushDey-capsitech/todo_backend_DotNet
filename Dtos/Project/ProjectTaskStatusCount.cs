using Projects.Dtos.Task;

namespace Projects.Dtos.Project
{
    public class ProjectTaskStatusCount
    {
        public string? name { get; set; }
        public List<ResponseStatusCount>? count { get; set; }
    }
}
