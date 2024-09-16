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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDto menuItemUpdateDto)
        {
            try
            {
                // Check if the DTO is valid
                if (menuItemUpdateDto == null || id != menuItemUpdateDto.Id)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid data", errors = ModelState });
                }

                // Check model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Fetch the menu item from the database
                MenuItem menuItemFromDb = await _context.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    return NotFound(new { message = "Menu item not found" });
                }

                // Update fields from DTO
                menuItemFromDb.Name = menuItemUpdateDto.Name;
                menuItemFromDb.Price = menuItemUpdateDto.Price;
                menuItemFromDb.Category = menuItemUpdateDto.Category;
                menuItemFromDb.SpecialTag = menuItemUpdateDto.SpecialTag;
                menuItemFromDb.Description = menuItemUpdateDto.Description;

                // If a new file is uploaded, handle file update
                if (menuItemUpdateDto.File != null && menuItemUpdateDto.File.Length > 0)
                {
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDto.File.FileName)}";

                    // Delete old image if exists
                    await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Container);

                    // Upload new image and update the image URL
                    menuItemFromDb.Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemUpdateDto.File);
                }

                // Save the changes
                _context.MenuItems.Update(menuItemFromDb);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK, menuItemFromDb);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the menu item.", details = ex.Message });
            }
        }

    }
}
