using System.Globalization;
using System.Web;

public static class CultureHelper
{
    public static CultureInfo GetCulture()
    {
        HttpRequest request = HttpContext.Current.Request;
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
}