using BidaTrader.Shared.Models;

namespace BidaTrader.Shared.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string? ThumbnailUrl { get; set; }

        public bool IsActive { get; set; }
        public int AccountId { get; set; }
        public int? StoreId { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<CommentDto> Comments { get; set; } = new();
        public Account? Account { get; set; }
        public Store? Store { get; set; }
    }

    public class CreatePostDto
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
    }

    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = "";
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Post? Post { get; set; }
        public Account? Account { get; set; }
    }

    public class PostPerPage
    {
        public List<PostDto> Posts { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}