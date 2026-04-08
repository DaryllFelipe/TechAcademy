using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechAcademyApi.Core.Entities
{
    /// <summary>
    /// Review Entity - Core Business Rule
    /// Represents a review for a restaurant
    /// Maintains relationship with Restaurant entity
    /// This is part of the Entities layer - the innermost, most stable layer
    /// </summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Restaurant")]
        public int RestaurantId { get; set; }

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Restaurant? Restaurant { get; set; }
    }
}
