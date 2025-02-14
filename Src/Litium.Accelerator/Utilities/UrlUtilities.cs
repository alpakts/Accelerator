using System;
using System.Text.RegularExpressions;

namespace Litium.Accelerator.Utilities
{
	/// <summary>
	/// Utility functions for urls.
	/// </summary>
	public class UrlUtilities
	{
		/// <summary>
		/// Adds or replaces a querystring parameter on provided url
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="newValue">The new value.</param>
		/// <returns>
		/// A new URL with specified parameter, if it exist the value is replaces otherwise it is added to the url
		/// </returns>
		public static string AddOrReplaceUrlParameter(Uri uri, string parameterName, string newValue)
		{
			return AddOrReplaceUrlParameter(uri.ToString(), parameterName, newValue);
		}

		/// <summary>
		/// Adds or replaces a querystring parameter on provided url
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="newValue">The new value.</param>
		/// <returns>
		/// A new URL with specified parameter, if it exist the value is replaces otherwise it is added to the url
		/// </returns>
		public static string AddOrReplaceUrlParameter(string url, string parameterName, string newValue)
		{
			string param = parameterName + "=";
			if (url.Contains(param))
			{
				return Regex.Replace(url, string.Format("({0}=.*?(?=&))|({0}=.*$)", parameterName), string.Format("{0}={1}", parameterName, newValue), RegexOptions.IgnoreCase);
			}
			return string.Format("{0}{1}{2}={3}", url, (url.Contains("?") ? "&" : "?"), parameterName, newValue);
		}

		/// <summary>
		/// Removes the parameter from provided url.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns></returns>
		public static string RemoveUrlParameter(string url, string parameterName)
		{
			string param = parameterName + "=";
			if (url.Contains(param))
			{
				string newUrl = Regex.Replace(url, string.Format("({0}=.*?[?=&])|({0}=.*$)", parameterName), string.Empty, RegexOptions.IgnoreCase);

				// Verify that the url dont end whith an ampersand
				if (newUrl.EndsWith("&"))
					newUrl = newUrl.Remove(newUrl.Length - 1, 1);

				// Verify that the url dont end with an questionmark
				if (newUrl.EndsWith("?"))
					newUrl = newUrl.Remove(newUrl.Length - 1, 1);

				return newUrl;
			}

			return url;
		}
    }
}
