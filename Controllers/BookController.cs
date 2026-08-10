using Projects.Dtos.Book;
using Projects.Models;
using Projects.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Projects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;

        public BookController(BookService booksService) => _bookService = booksService;

        [HttpGet]
        public async Task<List<Book>> Get() => await _bookService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Book>> Get(string id)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(UpsertBookDto newBook)
        {
            var book = await _bookService.CreateAsync(newBook);

            return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Book updatedBook)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedBook.Id = book.Id;

            await _bookService.UpdateAsync(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _bookService.RemoveAsync(id);

            return NoContent();
        }
    }
}
