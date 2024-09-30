using FoodService_API.Data;
using FoodService_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ApplicationDbContext context, ILogger<ShoppingCartController> logger)
        {
            _context = context;
            _logger = logger;
        }



        [HttpGet]
        public async Task<IActionResult> GetShoppingCart(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is null or empty.");
                    return StatusCode(StatusCodes.Status400BadRequest, "UserId is required.");
                }

                var shoppingCart = await _context.ShoppingCarts
                    .Include(u => u.CartItems)
                    .ThenInclude(u => u.MenuItem)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (shoppingCart == null)
                {
                    _logger.LogInformation("Shopping cart not found for user {UserId}.", userId);
                    return StatusCode(StatusCodes.Status404NotFound, "Shopping cart not found.");
                }

                if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
                }

                return StatusCode(StatusCodes.Status200OK, shoppingCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the shopping cart for user {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {

        
            try
            {
                var shoppingCart = await _context.ShoppingCarts
                    .Include(u => u.CartItems)
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                var menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == menuItemId);

                if (menuItem == null)
                {
                    _logger.LogWarning("MenuItem with Id {MenuItemId} not found.", menuItemId);
                    return StatusCode(StatusCodes.Status400BadRequest, "MenuItem not found.");
                }

                if (shoppingCart == null && updateQuantityBy > 0)
                {
                    // Create a new shopping cart and add cart item
                     ShoppingCart newCart = new()
                    {
                        UserId = userId
                    };
                    await _context.ShoppingCarts.AddAsync(newCart);
                    await _context.SaveChangesAsync();


                    //Add cart items
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = newCart.Id,
                        MenuItem = null
                    };

                    await _context.CartItems.AddAsync(newCartItem);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("New shopping cart and item created for user {UserId}", userId);
                    return StatusCode(StatusCodes.Status200OK, new { message = "Item successfully added to a new shopping cart.", newCart });
                }
                else
                {
                    // Shopping cart exists
                  CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(x => x.MenuItemId == menuItemId);

                    if (cartItemInCart == null)
                    {
                        //Item does not exist  in current cast
                        CartItem newCartItem = new()
                        {
                            MenuItemId = menuItemId,
                            Quantity = updateQuantityBy,
                            ShoppingCartId = shoppingCart.Id,
                            MenuItem = null
                        };

                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Cart item updated for user {UserId}, MenuItemId {MenuItemId}", userId, menuItemId);
                        return StatusCode(StatusCodes.Status200OK, new { message = "Item quantity updated successfully.", cartItemInCart });
                    }
                    else
                    {
                        //Item already exist in the shopping cart and we have to  uodate quantity
                        int newQuantity = cartItemInCart.Quantity + updateQuantityBy;

                        if(updateQuantityBy ==0 || newQuantity < 0)
                        {
                            //Remove cart item from cart and if it is the only item then remove cart
                            _context.CartItems.Remove(cartItemInCart);

                            if(shoppingCart.CartItems.Count() ==1)
                            {
                                _context.ShoppingCarts.Remove(shoppingCart);
                            }
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            cartItemInCart.Quantity = newQuantity;
                            await _context.SaveChangesAsync();
                        }


                       

                        _logger.LogInformation("New item added to existing cart for user {UserId}, MenuItemId {MenuItemId}", userId, menuItemId);
                        return StatusCode(StatusCodes.Status200OK,   new { message = "New item added to the existing shopping cart.", cartItemInCart });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding or updating item in cart for user {UserId}, MenuItemId {MenuItemId}", userId, menuItemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
