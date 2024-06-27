using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace blogsproject_1.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Image { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(2000, ErrorMessage = "Content cannot be longer than 2000 characters.")]
        public string Content { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
