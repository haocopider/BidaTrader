using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public class PaymentRequestDto
    {
        public long OrderId { get; set; }
        public long Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BankCode { get; set; }
        public string Local { get; set; }
    }
}
