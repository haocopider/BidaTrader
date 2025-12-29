namespace BidaTrader.Client.Pages.Components
{
    public class ImageHepler
    {
        public static string TransferImage(string img)
        {
            var imgP = img.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? img
                : $"https://localhost:7049{img}";
            return imgP;
        }
    }
}
