using Projects.Models;

namespace Projects.Dtos.Todo
{
    public class UpsertTodoDto :UpdateTodo
    {
        
        public bool IsDone { get; set; } = false;
    }
}