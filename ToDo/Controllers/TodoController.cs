using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Todo.Models;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly TodoContext _context;

    public TodoController(TodoContext context)
    {
        _context = context;
    }

    // GET: api/todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemModel>>> GetTodoItems()
    {
        return await _context.TodoItems.ToListAsync();
    }

    // GET: api/todo/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemModel>> GetTodoItem(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    // POST: api/todo
    [HttpPost]
    public async Task<ActionResult<TodoItemModel>> CreateTodoItem(TodoItemModel item)
    {
        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, item);
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoItem(int id, TodoItemModel updatedItem)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        todoItem.Name = updatedItem.Name;
        todoItem.IsComplete = updatedItem.IsComplete;

        _context.Entry(todoItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TodoItemExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TodoItemExists(int id)
    {
        return _context.TodoItems.Any(e => e.Id == id);
    }
}
