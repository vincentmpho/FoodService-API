﻿using Microsoft.AspNetCore.Identity;

namespace FoodService_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name  { get; set; }
    }
}
