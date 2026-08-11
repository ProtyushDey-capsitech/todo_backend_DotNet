namespace Projects.Dtos.Todo
{
    public class ResponseTaskData: UpsertTaskDto
    {
        public string? Id { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
