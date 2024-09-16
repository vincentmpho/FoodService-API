﻿using System.ComponentModel.DataAnnotations;

namespace FoodService_API.Models.DTOs
{
    public class MenuItemUpdateDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialTag { get; set; }
        public string Category { get; set; }
        [Range(1, int.MaxValue)]
        public double Price { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
