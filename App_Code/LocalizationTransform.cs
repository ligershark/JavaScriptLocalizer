using System;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Optimization;

/// <summary>
/// Summary description for LocalizationTransform
/// </summary>
public class LocalizationTransform : IBundleTransform
{
    private ResourceManager _manager;
    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private static Regex _regex = new Regex(@"::([a-z0-9-_\.]+)::", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public LocalizationTransform(Type resourceType)
        : this(new ResourceManager(resourceType)) { }

    public LocalizationTransform(ResourceManager resourceManager)
    {
        _manager = resourceManager;
    }

    public void Process(BundleContext context, BundleResponse response)
    {
        context.UseServerCache = false;
        context.HttpContext.Response.Cache.VaryByHeaders["Accept-Language"] = true;
        context.HttpContext.Response.Cache.SetValidUntilExpires(true);

        SetCulture(context);
        Localize(response);
    }

    private void SetCulture(BundleContext context)
    {
        try
        {
            if (context.HttpContext.Request.UserLanguages != null && context.HttpContext.Request.UserLanguages.Length > 0)
            {
                string language = context.HttpContext.Request.UserLanguages[0];
                _culture = CultureInfo.CreateSpecificCulture(language);
            }
        }
        catch { }
    }

    private void Localize(BundleResponse response)
    {
        StringBuilder sb = new StringBuilder(response.Content);

        foreach (Match match in _regex.Matches(response.Content))
        {
            string value = match.Groups[1].Value;
            string result = GetTranslation(match.Groups[1].Value);

            sb.Replace(match.Value, result);
        }

        response.Content = sb.ToString();
    }

    /// <summary>
    /// Cleans the localized script text from making invalid javascript.
    /// </summary>
    private string GetTranslation(string value)
    {
        string result = _manager.GetString(value, _culture);

        if (string.IsNullOrEmpty(result))
        {
            return "TEXT NOT FOUND (" + value + ")";
        }

        return result.Replace("'", "\\'")
                   .Replace("\\", "\\\\");
    }
}