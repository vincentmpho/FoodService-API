using FoodService_API.Data;
using FoodService_API.Models;
using FoodService_API.Models.DTOs;
using FoodService_API.Services;
using FoodService_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _context.OrderHeaders.Include(u => u.OrderDetails)
                     .ThenInclude(u => u.MenuItem)
                     .OrderByDescending(u => u.OrderHeaderId);

                if (!string.IsNullOrEmpty(userId))
                {
                    var results = orderHeaders.Where(u => u.ApplicationUserId == userId);
                }
                else
                {
                    var results = orderHeaders;
                }
                return StatusCode(StatusCodes.Status200OK, orderHeaders);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                if (id == 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }
                var orderHeaders = _context.OrderHeaders.Include(u => u.OrderDetails)
                     .ThenInclude(u => u.MenuItem)
                     .Where(u => u.OrderHeaderId == id);

                if (orderHeaders == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return StatusCode(StatusCodes.Status200OK, orderHeaders);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request. Please try again later.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderDto)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderDto.ApplicationUserId,
                    PickUpEmail = orderHeaderDto.PickUpEmail,
                    PickUpName = orderHeaderDto.PickUpName,
                    PickUpPhoneNumber = orderHeaderDto.PickUpPhoneNumber,
                    OrderTotal = orderHeaderDto.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDto.StripePaymentIntentID,
                    TotalItems = orderHeaderDto.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDto.Status) ? SD.status_pending : orderHeaderDto.Status,
                };

                if (ModelState.IsValid)
                {
                    _context.OrderHeaders.Add(order);
                    _context.SaveChanges();

                    foreach (var orderDetailDto in orderHeaderDto.OrderDetailsDto)
                    {
                        OrderDetails orderDetail = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDto.ItemName,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Price = orderDetailDto.Price,
                            Quantity = orderDetailDto.Quantity,
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }
                    _context.SaveChanges();
                    return StatusCode(StatusCodes.Status201Created, order);
                }

                // Return a Bad Request response if the model state is invalid
                return StatusCode(StatusCodes.Status400BadRequest,ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDto orderHeaderUpdateDto)
        {
            try
            {
                // Check if the incoming DTO is null or if the ID in the DTO does not match the ID from the route.
                if (orderHeaderUpdateDto == null || id != orderHeaderUpdateDto.OrderHeaderId)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,"Invalid OrderHeader data.");
                }

                // Retrieve the order from the database using the given ID.
                OrderHeader orderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(u => u.OrderHeaderId == id);

                // If the order is not found, return a "Not Found" response.
                if (orderFromDb == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound,"Order header not found.");
                }

                // Update fields if they are provided in the DTO.
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpName))
                {
                    orderFromDb.PickUpName = orderHeaderUpdateDto.PickUpName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpPhoneNumber))
                {
                    orderFromDb.PickUpPhoneNumber = orderHeaderUpdateDto.PickUpPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpEmail))
                {
                    orderFromDb.PickUpEmail = orderHeaderUpdateDto.PickUpEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.Status))
                {
                    orderFromDb.Status = orderHeaderUpdateDto.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.StripePaymentIntentID))
                {
                    orderFromDb.StripePaymentIntentID = orderHeaderUpdateDto.StripePaymentIntentID;
                }

                // Save changes to the database.
                await _context.SaveChangesAsync();

                // Return "No Content" status code to indicate successful update.
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                // Return "Internal Server Error" status code.
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order header.");
            }
        }



    }
}
