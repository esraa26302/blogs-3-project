using blogsproject_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace blogsproject_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<CommentController> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Include(c => c.Replies)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments
                
                .Include(c => c.Replies)
              
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(CommentCreateDto commentDto)
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
            _logger.LogInformation("Creating comment for user ID: {UserId}", userId);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            int postId;
            if (commentDto.ParentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(commentDto.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return BadRequest(new { message = "Parent comment not found." });
                }
                postId = parentComment.PostId;  
            }
            else if (commentDto.PostId.HasValue)
            {
                postId = commentDto.PostId.Value;
                var post = await _context.Posts.FindAsync(postId);
                if (post == null)
                {
                    return BadRequest(new { message = "Post not found." });
                }
            }
            else
            {
                return BadRequest(new { message = "Post ID or Parent Comment ID must be provided." });
            }

            var comment = new Comment
            {
                Content = commentDto.Content,
                Title = commentDto.Title,
                UserId = userId,
                PostId = postId,
                User = user,
                ParentCommentId = commentDto.ParentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutComment(int id, CommentUpdateDto commentUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found." });
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);
            if (comment.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to update this comment." });
            }

            comment.Title = commentUpdateDto.Title;
            comment.Content = commentUpdateDto.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = int.Parse(userIdClaim.Value);
            if (comment.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to delete this comment." });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }

    



    }
}