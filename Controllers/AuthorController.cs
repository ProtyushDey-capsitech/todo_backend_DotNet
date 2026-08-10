using Projects.Dtos.Author;
using Projects.Models;
using Projects.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projects.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly AuthorService _authorService;

        public AuthorController(AuthorService authorService) => _authorService = authorService;

        [HttpGet]
        public async Task<ActionResult<dynamic>> Get([FromQuery] AuthorQueryDto query)
        {
            var result = await _authorService.GetPaginatedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Author>> Get(string id)
        {
            var author = await _authorService.GetAsync(id);
            return author is null ? NotFound() : Ok(author);
        }

        [HttpPost]
        public async Task<IActionResult> Post(UpsertAuthorDto newAuthor)
        {
            var result = await _authorService.CreateAsync(newAuthor);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, UpsertAuthorDto updatedAuthor)
        {
            var existing = await _authorService.GetAsync(id);
            if (existing is null)
                return NotFound();

            await _authorService.UpdateAsync(id, updatedAuthor);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _authorService.GetAsync(id);
            if (existing is null)
                return NotFound();

            await _authorService.RemoveAsync(id);
            return NoContent();
        }
    }
}
