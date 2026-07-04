using api.Models;
using api.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{UserRolesDto.Admin},{UserRolesDto.People}")]
    public class PeopleController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        const string GetPersonRouteName = "GetPerson";

        [HttpPost]
        public async Task<IActionResult> AddPerson(Person person)
        {
            try
            {
                await _context.AddAsync(person);
                await _context.SaveChangesAsync();
                return CreatedAtRoute(GetPersonRouteName, new { id = person.Id }, person);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "An error occurred while adding the person.");
            }
        }

        [HttpGet("{id:int}", Name = GetPersonRouteName)]
        public async Task<IActionResult> GetPerson([FromRoute] int id)
        {
            try
            {
                Person? person = await _context.FindAsync<Person>(id);
                if (person is null)
                {
                    return NotFound($"Person with ID {id} not found.");
                }
                return Ok(person);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "An error occurred while retrieving the person.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                List<Person> people = await _context.People.ToListAsync();
                return Ok(people);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "An error occurred while retrieving the people.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutPerson([FromRoute] int id, [FromBody] Person person)
        {
            try
            {
                Person? existingPerson = await _context.FindAsync<Person>(id);
                if (existingPerson is null)
                {
                    return NotFound($"Person with ID {id} not found.");
                }
                _context.Entry(existingPerson).CurrentValues.SetValues(person);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "An error occurred while updating the person.");
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePerons([FromRoute] int id)
        {
            try
            {
                await _context.People.Where(p => p.Id == id).ExecuteDeleteAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "An error occurred while deleting the person.");
            }
        }




    }
}
