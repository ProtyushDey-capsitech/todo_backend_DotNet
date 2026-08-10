using Projects.Dtos.Common;

namespace Projects.Dtos.Book
{
    public class BookQueryDto : PaginatedQueryDto
    {
        public List<string> AuthorIds { get; set; } = new List<string>();

        public List<string> GenreIds { get; set; } = new List<string>();
    }
}
