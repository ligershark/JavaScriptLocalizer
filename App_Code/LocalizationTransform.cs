using System;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

public class LocalizationTransform : IBundleTransform
{
    private ResourceManager _manager;
    private Regex _regex;

    public LocalizationTransform(Type resourceType)
        : this(new ResourceManager(resourceType)) { }

    public LocalizationTransform(Type resourceType, Regex regex)
        : this(new ResourceManager(resourceType), regex) { }

    public LocalizationTransform(ResourceManager resourceManager)
        : this(resourceManager, new Regex(@"::([a-z0-9-_\.]+)::", RegexOptions.IgnoreCase | RegexOptions.Compiled)) { }

    public LocalizationTransform(ResourceManager resourceManager, Regex regex)
    {
        _manager = resourceManager;
        _regex = regex;
    }

    public void Process(BundleContext context, BundleResponse response)
    {
        context.UseServerCache = false;

        if (context.HttpContext.CurrentHandler.ToString().Contains("BundleHandler"))
        {
            context.HttpContext.Response.Cache.VaryByParams["lang"] = true;
            context.HttpContext.Response.Cache.VaryByParams["v"] = true;
            context.HttpContext.Response.Cache.SetValidUntilExpires(true);
            context.HttpContext.Response.Cache.SetCacheability(response.Cacheability);
        }

        Localize(response);
    }

    private void Localize(BundleResponse response)
    {
        StringBuilder sb = new StringBuilder(response.Content);
        CultureInfo culture = GetCulture(HttpContext.Current);

        foreach (Match match in _regex.Matches(response.Content))
        {
            string result = GetTranslation(match.Groups[1].Value, culture);
            sb.Replace(match.Value, result);
        }

        response.Content = sb.ToString();
    }

    public static CultureInfo GetCulture(HttpContext context)
    {
        HttpRequest request = context.Request;
        string language = request.QueryString["lang"];

        if (string.IsNullOrEmpty(language) && request.UserLanguages != null && request.UserLanguages.Length > 0)
        {
            language = request.UserLanguages[0];
        }

        try
        {
            if (!string.IsNullOrEmpty(language))
            {
                return CultureInfo.CreateSpecificCulture(language);
            }
        }
        catch { }

        return CultureInfo.CurrentCulture;
    }

    private string GetTranslation(string value, CultureInfo culture)
    {
        string result = _manager.GetString(value, culture) ?? "TEXT NOT FOUND (" + value + ")";

        return result.Replace("'", "\\'")
                     .Replace("\\", "\\\\");
    }
}