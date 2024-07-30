namespace blogsproject_1.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public DateTime CreationDate { get; set; } 
        public List<Comment> Comments { get; set; }
    }
}
