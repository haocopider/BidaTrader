

namespace BidaTrader.Shared.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public bool? IsActive { get; set; }

        public bool? IsCommentEnabled { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
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
