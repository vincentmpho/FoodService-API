using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodService_API.Models.DTOs
{
    public class OrderDetailsCreateDto
    {
      
        [Required]
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
