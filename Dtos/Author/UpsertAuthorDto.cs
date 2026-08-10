using Projects.Models;

namespace Projects.Dtos.Author
{
    public class UpsertAuthorDto
    {
        public PersonName Name { get; set; } = new PersonName();

        public Projects.Models.Author MapDtoToModel(string? id = null)
        {
            return new Projects.Models.Author
            {
                Id = id,
                Name = new PersonName
                {
                    First = Name.First,
                    Middle = Name.Middle,
                    Last = Name.Last,
                },
            };
        }
    }
}
