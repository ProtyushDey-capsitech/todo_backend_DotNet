namespace Projects.Dtos.Book
{
    public class UpsertBookDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<string> AuthorIds { get; set; } = [];
        public List<string> GenreIds { get; set; } = [];
        public string? ISBN { get; set; }
        public DateTime? PublishedOn { get; set; }
        public int? Pages { get; set; }
        public decimal? Price { get; set; }
        public bool InStock { get; set; } = true;
    }
}
