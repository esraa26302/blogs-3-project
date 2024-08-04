using System.ComponentModel.DataAnnotations;

namespace blogsproject_1.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot be longer than 1000 characters.")]
        public string Content { get; set; }
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string? Title { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

       
        public int? ParentCommentId { get; set; }
        public Comment ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    }
}
