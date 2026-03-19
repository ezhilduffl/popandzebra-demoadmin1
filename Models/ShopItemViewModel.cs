using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PopZebra.Models
{
    public class ShopItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        public IFormFile ImageFile { get; set; }

        // Shown on edit
        public string ExistingImagePath { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}