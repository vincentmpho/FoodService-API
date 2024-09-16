using FoodService_API.Data;
using FoodService_API.Models;
using FoodService_API.Models.DTOs;
using FoodService_API.Services;
using FoodService_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobService _blobService;

        public MenuItemController(ApplicationDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
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

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem([FromForm] MenuItemCreateDto menuItemCreateDto)
        {
            try
            {
                // Check if the basic model validation is successful
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid data", errors = ModelState });
                }

                // Custom validation for the file (check if the file is provided and has content)
                if (menuItemCreateDto.File == null || menuItemCreateDto.File.Length == 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "File is required and cannot be empty." });
                }

                string filenName = $"{Guid.NewGuid()} {Path.GetExtension(menuItemCreateDto.File.FileName)}";

                
                var menuItem = new MenuItem
                {
                    Name = menuItemCreateDto.Name,
                    Description = menuItemCreateDto.Description,
                    SpecialTag = menuItemCreateDto.SpecialTag,
                    Category = menuItemCreateDto.Category,
                    Price = menuItemCreateDto.Price,
                    Image =  await _blobService.UploadBlob(filenName,SD.SD_Storage_Container, menuItemCreateDto.File)
                };

                // Save the MenuItem to the database
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created, menuItem);
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error occurred: {ex.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the menu item.", details = ex.Message });
            }
        }


    }
}
