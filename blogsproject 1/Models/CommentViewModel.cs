namespace blogsproject_1.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public List<CommentViewModel> Replies { get; set; }
    }
}
