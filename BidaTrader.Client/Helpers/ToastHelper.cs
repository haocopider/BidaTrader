namespace BidaTrader.Client.Helpers
{
    public class ToastHelper
    {
        public event Action<string, string, string>? OnShow;

        public void Show(string title, string message, string type = "success")
        {
            OnShow?.Invoke(title, message, type);
        }
    }

}
