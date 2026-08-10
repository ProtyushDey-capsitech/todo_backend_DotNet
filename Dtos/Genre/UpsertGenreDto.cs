namespace Projects.Dtos.Genre
{
    public class UpsertGenreDto
    {
        public string Name { get; set; } = null!;

        public Projects.Models.Genre MapDtoToModel(string? id = null)
        {
            return new Projects.Models.Genre { Id = id, Name = Name };
        }
    }
}
