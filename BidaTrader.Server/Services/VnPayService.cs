using Azure;
using BidaTrader.Server.Helpers;
using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BidaTrader.Server.Services
{

    public class VnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(HttpContext context, PaymentRequestDto order)
        {
            // --- SỬA LẠI CÁCH LẤY CONFIG (Thêm "VnPay:" ở trước) ---
            string vnp_Returnurl = _configuration["VnPay:vnp_Returnurl"];
            string vnp_Url = _configuration["VnPay:vnp_Url"];
            string vnp_TmnCode = _configuration["VnPay:vnp_TmnCode"];
            string vnp_HashSecret = _configuration["VnPay:vnp_HashSecret"];

            // Kiểm tra null để dễ debug
            if (string.IsNullOrEmpty(vnp_HashSecret) || string.IsNullOrEmpty(vnp_TmnCode))
            {
                throw new Exception("Vui lòng cấu hình VnPay trong appsettings.json");
            }

            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
            if (!string.IsNullOrEmpty(order.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", order.BankCode);
            }

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            var ipAddr = Utils.GetIpAddress(context);
            if (string.IsNullOrEmpty(ipAddr) || ipAddr == "::1") ipAddr = "127.0.0.1";
            vnpay.AddRequestData("vnp_IpAddr", ipAddr);

            // Locale
            if (!string.IsNullOrEmpty(order.Local))
            {
                vnpay.AddRequestData("vnp_Locale", order.Local);
            }
            else
            {
                vnpay.AddRequestData("vnp_Locale", "vn");
            }

            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + order.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

            // Tạo Url thanh toán
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }
    
        public (bool, long, string) PayReturn(IQueryCollection request)
        {
            var vnpayData = request;
            string vnp_HashSecret = _configuration["VnPay:vnp_HashSecret"];
            var vnpayLibrary = new VnPayLibrary();

            foreach (var s in vnpayData)
            {
                if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                {
                    vnpayLibrary.AddResponseData(s.Key, s.Value);
                }
            }

            long vnp_TxnRef = Convert.ToInt64(vnpayLibrary.GetResponseData("vnp_TxnRef"));
            long vnp_Amount = Convert.ToInt64(vnpayLibrary.GetResponseData("vnp_Amount")) / 100;
            string vnp_ResponseCode = vnpayLibrary.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = request["vnp_SecureHash"];

            bool checkSignature = vnpayLibrary.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            return (checkSignature, vnp_TxnRef, vnp_ResponseCode);
        }
    }

}