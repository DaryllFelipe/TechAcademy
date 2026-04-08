using System.ComponentModel.DataAnnotations;

namespace TechAcademyApi.Core.Entities
{
    /// <summary>
    /// Restaurant Entity - Core Business Rule
    /// Represents a restaurant in the system
    /// This is part of the Entities layer - the innermost, most stable layer
    /// </summary>
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
