using BidaTrader.Server.Services;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidaTrader.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IService<Post> _postService;

        public PostsController(IService<Post> service)
        {
            _postService = service;
        }

        [HttpGet]
        public async Task<ActionResult> GetPosts([FromQuery] string? title, [FromQuery] string? author, [FromQuery] bool? isActive, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            var (posts, totalItems) = await ((PostService)_postService).GetPostWithPagnination(title, author, isActive, pageIndex, pageSize);

            var dtos = posts.Select(p => new PostDto
            {
                Id = p.Id,
                AccountId = p.AccountId,
                Title = p.Title,
                Content = p.Content,
                ThumbnailUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt ?? DateTime.Now,
            }).ToList();

            var response = new PostPerPage
            {
                Posts = dtos,
                TotalCount = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetPostById(int id)
        {
            var post = await _postService.GetItemByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var dto = new PostDto
            {
                Id = post.Id,
                AccountId = post.AccountId,
                Title = post.Title,
                Content = post.Content,
                ThumbnailUrl = post.ImageUrl,
                IsActive = post.IsActive,
                CreatedAt = post.CreatedAt ?? DateTime.Now,
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDto postDto)
        {
            var post = new Post
            {
                AccountId = postDto.AccountId,
                Title = postDto.Title,
                Content = postDto.Content,
                ImageUrl = postDto.ThumbnailUrl,
                IsActive = postDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await _postService.CreateItemAsync(post);
            if (!created)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo bài viết");
            }
            return Ok("Tạo bài viết thành công");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] PostDto postDto)
        {
            if (id != postDto.Id)
            {
                return BadRequest("ID không khớp");
            }
            var existingPost = await _postService.GetItemByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }
            existingPost.Title = postDto.Title;
            existingPost.Content = postDto.Content;
            existingPost.ImageUrl = postDto.ThumbnailUrl;
            existingPost.IsActive = postDto.IsActive;
            existingPost.UpdatedAt = DateTime.UtcNow;
            var updated = await _postService.UpdateItemAsync(existingPost);
            if (!updated)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi cập nhật bài viết");
            }
            return Ok("Cập nhật bài viết thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var existingPost = await _postService.GetItemByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }
            await _postService.DeleteItemAsync(id);
            return Ok("Xóa bài viết thành công");
        }
    }
}