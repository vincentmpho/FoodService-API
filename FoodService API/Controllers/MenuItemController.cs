using FoodService_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetMenuItems()
        {
            try
            {
                var menuItems = await _context.MenuItems.ToListAsync();
                return StatusCode(StatusCodes.Status200OK, menuItems);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error occurred: {ex.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving menu items.", details = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetMenuItemById(int id)
        {
            if (id == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid ID supplied.");
            }

            try
            {
                var menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);

                if (menuItem == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"MenuItem with ID {id} not found."); 
                }

                return StatusCode(StatusCodes.Status200OK, menuItem); 
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error occurred: {ex.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the menu item.", details = ex.Message });
            }
        }
    }
}
