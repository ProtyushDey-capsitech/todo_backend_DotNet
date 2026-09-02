using Projects.Dtos.Common;

namespace Projects.Dtos.Project
{
    public class ProjectPaginatedQueryDto: PaginatedQueryDto
    {
        public string? Status { get; set; }
    }
}
