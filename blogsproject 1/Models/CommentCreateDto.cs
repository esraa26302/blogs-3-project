namespace blogsproject_1.Models
{
    public class CommentCreateDto
    {
        public string Content { get; set; }
        public string Title { get; set; }
     
        public int? PostId { get; set; }

      
        public int? ParentCommentId { get; set; }
    }
}
