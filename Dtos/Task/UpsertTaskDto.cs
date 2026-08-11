using Projects.Models;

namespace Projects.Dtos.Todo
{
    public class UpsertTaskDto :UpdateTask
    {

        public string? Status { get; set; }
    }
}