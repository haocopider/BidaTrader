using Microsoft.AspNetCore.Components;

namespace BidaTrader.Client.Helpers
{
    public static class NavigationManagerExtensions
    {
        /// <summary>
        /// Navigate với fragment (#xxx) và query phía sau fragment
        /// Ví dụ: #posts?author=A/p2
        /// </summary>
        public static void NavigateWithFragmentQuery(
            this NavigationManager navigation,
            string fragment,
            string? query = null,
            bool replace = true)
        {
            if (string.IsNullOrWhiteSpace(fragment))
                return;

            var uri = new Uri(navigation.Uri);
            var baseUrl = uri.GetLeftPart(UriPartial.Path);

            var finalUrl = string.IsNullOrWhiteSpace(query)
                ? $"{baseUrl}{fragment}"
                : $"{baseUrl}{fragment}?{query}";

            navigation.NavigateTo(finalUrl, replace);
        }
    }
}
