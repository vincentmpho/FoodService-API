using FoodService_API.Data;
using FoodService_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PaymentController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _context.ShoppingCarts
                .Include(u => u.CartItems)
                .ThenInclude(u => u.MenuItem)
                .FirstOrDefault(u => u.UserId == userId);

            if (shoppingCart == null || shoppingCart.CartItems == null || !shoppingCart.CartItems.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest,"Shopping cart is empty or does not exist.");
            }

            #region Create Payment Intent

            StripeConfiguration.ApiKey = _configuration["stripeSettings:SecretKey"];

            shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);

            PaymentIntentCreateOptions options = new()
            {
                Amount = (int)(shoppingCart.CartTotal * 100), 
                Currency = "usd",
                PaymentMethodTypes = new List<string>
                {
                    // Ensure payment method type is in lowercase
                    "card", 
                },
            };

            PaymentIntentService service = new();
            PaymentIntent response = await service.CreateAsync(options); 

            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;

            #endregion

            // Save changes to the database if necessary
            _context.Update(shoppingCart);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK,shoppingCart);
        }
    }
}
