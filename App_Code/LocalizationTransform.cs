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
    private CultureInfo _culture;
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
        _culture = CultureInfo.CurrentCulture;
    }

    public void Process(BundleContext context, BundleResponse response)
    {
        context.UseServerCache = false;

        if (string.IsNullOrEmpty(context.HttpContext.Request.QueryString["lang"]))
        {
            context.HttpContext.Response.Cache.VaryByHeaders["Accept-Language"] = true;
        }

        context.HttpContext.Response.Cache.VaryByParams["lang"] = true;
        context.HttpContext.Response.Cache.VaryByParams["v"] = true;
        context.HttpContext.Response.Cache.SetValidUntilExpires(true);
            
        _culture = GetCulture(HttpContext.Current);

        Localize(response);
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
            return CultureInfo.CreateSpecificCulture(language);
        }
        catch { }

        return CultureInfo.CurrentCulture;
    }

    private void Localize(BundleResponse response)
    {
        StringBuilder sb = new StringBuilder(response.Content);

        foreach (Match match in _regex.Matches(response.Content))
        {
            string result = GetTranslation(match.Groups[1].Value);
            sb.Replace(match.Value, result);
        }

        response.Content = sb.ToString();
    }

    private string GetTranslation(string value)
    {
        string result = _manager.GetString(value, _culture) ?? "TEXT NOT FOUND (" + value + ")";

        return result.Replace("'", "\\'")
                     .Replace("\\", "\\\\");
    }
}