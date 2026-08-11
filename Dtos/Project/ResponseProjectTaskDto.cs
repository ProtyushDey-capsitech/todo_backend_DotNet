using Projects.Dtos.Todo;

namespace Projects.Dtos.Project
{
    public class ResponseProjectTaskDto:ResponseProjectData
    {
        public List<ResponseTaskData>? Tasks {  get; set; }
    }
}
