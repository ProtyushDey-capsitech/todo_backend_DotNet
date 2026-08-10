using Projects.Dtos.Common;

namespace Projects.Dtos.Author
{
    public class AuthorQueryDto : PaginatedQueryDto
    {
        public string? BookTitle { get; set; }
    }
}
