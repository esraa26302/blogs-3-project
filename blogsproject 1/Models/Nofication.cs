namespace blogsproject_1.Models
{
    public class Nofication
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public int AdminUserId { get; set; }
        public User AdminUser { get; set; }
    }
}
