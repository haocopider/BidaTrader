using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Post? Post { get; set; }
        public Account? Account { get; set; }
    }
}
