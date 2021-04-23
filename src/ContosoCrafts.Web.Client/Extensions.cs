using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace ContosoCrafts.Web.Client
{
    public static class Extensions
    {
        public static Dictionary<string, StringValues> GetQueryString(this NavigationManager navManager)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);
            return QueryHelpers.ParseQuery(uri.Query);

        }
        public static string GetQueryString(this NavigationManager navManager, string key)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);
            QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var queryValue);
            return queryValue.ToString();
        }
    }
}