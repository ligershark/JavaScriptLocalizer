#region Using

using System;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Text.RegularExpressions;
using System.Resources;
using System.IO.Compression;

#endregion

public class ScriptTranslator : IHttpHandler
{
	#region IHttpHandler Members

	/// <summary>
	/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
	/// </summary>
	/// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
	public bool IsReusable
	{
		get { return false; }
	}

	/// <summary>
	/// Enables processing of HTTP Web requests by a custom HttpHandler that implements 
	/// the <see cref="T:System.Web.IHttpHandler"></see> interface.
	/// </summary>
	/// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that 
	/// provides references to the intrinsic server objects 
	/// (for example, Request, Response, Session, and Server) used to service HTTP requests.
	/// </param>
	public void ProcessRequest(HttpContext context)
	{
		string relativePath = context.Request.AppRelativeCurrentExecutionFilePath.Replace(".axd", string.Empty);
		string absolutePath = context.Server.MapPath(relativePath);
		string script = ReadFile(absolutePath);
		string translated = TranslateScript(script);
		
		context.Response.Write(translated);

		Compress(context);
		SetHeadersAndCache(absolutePath, context);		
	}

	#endregion

	/// <summary>
	/// This will make the browser and server keep the output
	/// in its cache and thereby improve performance.
	/// </summary>
	private void SetHeadersAndCache(string file, HttpContext context)
	{
		//context.Response.ContentType = "text/javascript";
		context.Response.AddFileDependency(file);
		context.Response.Cache.VaryByHeaders["Accept-Language"] = true;
		context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
		context.Response.Cache.SetLastModifiedFromFileDependencies();
		context.Response.Cache.SetExpires(DateTime.Now.AddDays(7));
		context.Response.Cache.SetValidUntilExpires(true);
		context.Response.Cache.SetCacheability(HttpCacheability.Public);
	}

	#region Localization

	private static Regex REGEX = new Regex(@"Translate\(([^\))]*)\)", RegexOptions.Singleline | RegexOptions.Compiled);

	/// <summary>
	/// Translates the text keys in the script file. The format is Translate(key).
	/// </summary>
	/// <param name="text">The text in the script file.</param>
	/// <returns>A localized version of the script</returns>
	private string TranslateScript(string text)
	{
		MatchCollection matches = REGEX.Matches(text);
		ResourceManager manager = new ResourceManager(typeof(Resources.text));

		foreach (Match match in matches)
		{
			object obj = manager.GetObject(match.Groups[1].Value);
			if (obj != null)
			{
				text = text.Replace(match.Value, CleanText(obj.ToString()));
			}
		}

		return text;
	}

	/// <summary>
	/// Cleans the localized script text from making invalid javascript.
	/// </summary>
	private static string CleanText(string text)
	{
		text = text.Replace("'", "\\'");
		text = text.Replace("\\", "\\\\");

		return text;
	}

	/// <summary>
	/// Reads the content of the specified file.
	/// </summary>
	private static string ReadFile(string absolutePath)
	{
		if (File.Exists(absolutePath))
		{
			using (StreamReader reader = new StreamReader(absolutePath))
			{
				return reader.ReadToEnd();		
			}
		}

		return null;
	}

	#endregion

	#region Compression

	private const string GZIP = "gzip";
	private const string DEFLATE = "deflate";

	/// <summary>
	/// Compresses the HTTP response.
	/// </summary>
	private static void Compress(HttpContext context)
	{
		if (IsEncodingAccepted(DEFLATE, context))
		{
			context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
			SetEncoding(DEFLATE, context);
		}
		else if (IsEncodingAccepted(GZIP, context))
		{
			context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
			SetEncoding(GZIP, context);
		}
	}

	/// <summary>
	/// Checks the request headers to see if the specified
	/// encoding is accepted by the client.
	/// </summary>
	private static bool IsEncodingAccepted(string encoding, HttpContext context)
	{
		return context.Request.Headers["Accept-encoding"] != null && context.Request.Headers["Accept-encoding"].Contains(encoding);
	}

	/// <summary>
	/// Adds the specified encoding to the response headers.
	/// </summary>
	private static void SetEncoding(string encoding, HttpContext context)
	{
		context.Response.AppendHeader("Content-encoding", encoding);
	}

	#endregion
}
