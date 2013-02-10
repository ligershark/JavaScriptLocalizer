using System.Linq;
using System.Web;
using System.Web.Optimization;

public static class LocScripts
{
    public static IHtmlString Render(params string[] paths)
    {
        string culture = LocalizationTransform.GetCulture(HttpContext.Current).Name;

        foreach (string path in paths.Where(p => p.StartsWith("~/")))
        {
            if (!BundleTable.EnableOptimizations)
            {
                var locPaths = paths.Select(p => p + "?lang=" + culture);
                return Scripts.Render(locPaths.ToArray());
            }
        }

        string tag = Scripts.Render(paths).ToString().Replace("?v=", "?lang=" + culture + "&v=");

        return new HtmlString(tag);
    }
}