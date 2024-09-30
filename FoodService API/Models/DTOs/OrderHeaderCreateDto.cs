using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodService_API.Models.DTOs
{
    public class OrderHeaderCreateDto
    {
 
        [Required]
        public string PickUpName { get; set; }
        [Required]
        public string PickUpPhoneNumber { get; set; }
        [Required]
        public string PickUpEmail { get; set; }
        public string ApplicationUserId { get; set; }
        public double OrderTotal { get; set; }

        public string StripePaymentIntentID { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }
        public IEnumerable<OrderDetailsCreateDto> OrderDetailsDto { get; set; }
    }
}
