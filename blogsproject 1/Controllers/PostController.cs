using blogsproject_1.Models;
using blogsproject_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace blogsproject_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAngularApp")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public PostController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostViewModel>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    Content = p.Content,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    UserImage = p.User.Image,
                    CreationDate = p.CreationDate
                })
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();

            return Ok(posts);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<PostViewModel>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    Content = p.Content,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    UserImage = p.User.Image,
                    CreationDate = p.CreationDate,
                    Comments = p.Comments.Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Title = c.Title,
                        UserId = c.UserId,
                        UserName = c.User.Name,
                        UserImage = c.User.Image,
                        Replies = c.Replies.Select(r => new CommentViewModel
                        {
                            Id = r.Id,
                            Content = r.Content,
                            Title = r.Title,
                            UserId = r.UserId,
                            UserName = r.User.Name,
                            UserImage = r.User.Image
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }



        [HttpPost]
        [Authorize(Policy = "WriterOrAdminPolicy")]

        public async Task<ActionResult<Post>> PostPost(PostCreateDto postDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            var post = new Post
            {
                Image = postDto.Image,
                Content = postDto.Content,
                UserId = userId,
                User = user,
                Comments = new List<Comment>()
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var adminUsers = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
            foreach (var admin in adminUsers)
            {
                await _notificationService.SendNotificationAsync(admin.OneId.ToString(), "New Post Created ", $"Writer {user.Name} with id {user.Id} and email {user.Email} has created a new post.");
            }

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }



        [HttpPut("{id}")]
        [Authorize(Policy = "WriterOrAdminPolicy")]
        public async Task<IActionResult> PutPost(int id, PostUpdateDto postDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);
            var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userRole = userRoleClaim?.Value;

            var post = await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }

            if (post.UserId != userId && userRole != "Admin")
            {
                return StatusCode(403, new { message = "You are not authorized to update this post." });
            }

            post.Image = postDto.Image;
            post.Content = postDto.Content;

            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "WriterOrAdminPolicy")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);
            var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userRole = userRoleClaim?.Value;

            var post = await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }

            if (post.UserId != userId && userRole != "Admin")
            {
                return StatusCode(403, new { message = "You are not authorized to delete this post." });
            }

           
            _context.Comments.RemoveRange(post.Comments);

           
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    }