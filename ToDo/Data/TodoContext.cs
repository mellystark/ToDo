using Microsoft.EntityFrameworkCore;
using ToDo.Models;
using Todo.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } // Kullanıcı DbSet'i

    // Diğer DbSet'ler...
    public DbSet<TodoItemModel> TodoItems { get; set; }
}
