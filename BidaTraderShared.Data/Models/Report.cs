using System;
using System.Collections.Generic;

namespace BidaTraderShared.Data.Models;

public partial class Report
{
    public int Id { get; set; }

    public int ReporterId { get; set; }

    public string? TargetType { get; set; }

    public int TargetId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Reporter { get; set; } = null!;
}
