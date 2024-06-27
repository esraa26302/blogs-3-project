using blogsproject_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace blogsproject_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts.Include(p => p.User).Include(p => p.Comments).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts.Include(p => p.User).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }


        [HttpPost]
        [Authorize(Policy = "WriterPolicy")]
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

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "WriterPolicy")]
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

            
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

           
            var post = await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }

          
            if (post.UserId != userId)
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
        [Authorize(Policy = "WriterPolicy")]
        public async Task<IActionResult> DeletePost(int id)
        {
            
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);

         
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }


            if (post.UserId != userId)
            {
                return StatusCode(403, new { message = "You are not authorized to delete this post." });
            }

           
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}