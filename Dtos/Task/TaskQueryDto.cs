using Projects.Dtos.Common;

namespace Projects.Dtos.Task
{
    public class TaskQueryDto : PaginatedQueryDto
    {
        public string? Name { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
