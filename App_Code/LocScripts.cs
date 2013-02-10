using System.Linq;
using System.Web;
using System.Web.Optimization;

public static class LocScripts
{
	public static IHtmlString Render(params string[] paths)
	{
		string culture = CultureHelper.GetCulture().Name;

		if (!BundleTable.EnableOptimizations)
		{			
			var locPaths = paths.Select(p => p + "?lang=" + culture);
			return Scripts.Render(locPaths.ToArray());
		}

		string tag = Scripts.Render(paths).ToString().Replace("?v=", "?lang=" + culture + "&v=");
		return new HtmlString(tag);
	}

	public static IHtmlString Render(bool insertLanguage, params string[] paths)
	{
		if (insertLanguage)
			return Render(paths);

		return Scripts.Render(paths);
	}
}