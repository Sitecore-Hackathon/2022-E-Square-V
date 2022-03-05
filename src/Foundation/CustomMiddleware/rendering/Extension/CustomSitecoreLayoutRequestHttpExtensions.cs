using Sitecore.LayoutService.Client.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Mvp.Foundation.CustomMiddleware
{
    public static class CustomSitecoreLayoutRequestHttpExtensions
    {
		private static readonly List<string> _defaultSitecoreRequestKeys = new List<string> { "sc_site", "item", "sc_lang", "sc_apikey", "sc_mode", "sc_date" };

		/// <summary>
		/// Build an URI using the default Sitecore layout entries in the provided request.
		/// </summary>
		/// <param name="request">The request object.</param>
		/// <param name="baseUri">The base URI used to compose the final URI.</param>
		/// <returns>An URI containing the base URI and the relevant entries in the request object added as query strings.</returns>
		public static Uri BuildDefaultSitecoreLayoutRequestUri(this SitecoreLayoutRequest request, Uri baseUri)
		{
			return BuildUri(request, baseUri, _defaultSitecoreRequestKeys);
		}

		/// <summary>
		/// Build an URI using the default Sitecore layout entries in the provided request.
		/// </summary>
		/// <param name="request">The request object.</param>
		/// <param name="baseUri">The base URI used to compose the final URI.</param>
		/// <param name="additionalQueryParameters">The additional URI query parameters to get from the request.</param>
		/// <returns>An URI containing the base URI and the relevant entries in the request object added as query strings.</returns>
		public static Uri BuildDefaultSitecoreLayoutRequestUri(this SitecoreLayoutRequest request, Uri baseUri, IEnumerable<string> additionalQueryParameters)
		{
			List<string> list = new List<string>(_defaultSitecoreRequestKeys);
			list.AddRange(additionalQueryParameters);
			return BuildUri(request, baseUri, list);
		}

		/// <summary>
		/// Build an URI using all the entries in the provided request.
		/// </summary>
		/// <param name="request">The request object.</param>
		/// <param name="baseUri">The base URI used to compose the final URL.</param>
		/// <param name="queryParameters">The URI query parameters to get from request.</param>
		/// <returns>An URI containing the base URI and all the valid entries in the request object added as query strings.</returns>
		public static Uri BuildUri(this SitecoreLayoutRequest request, Uri baseUri, IEnumerable<string> queryParameters)
		{
			IEnumerable<string> queryParameters2 = queryParameters;
			string[] array = (from entry in request.Where((KeyValuePair<string, object> entry) => queryParameters2.Contains<string>(entry.Key)).ToList()
							  where entry.Value is string && !string.IsNullOrWhiteSpace(entry.Value.ToString())
							  select entry into kvp
							  select WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value.ToString())).ToArray();
			if (!array.Any())
			{
				return baseUri;
			}
			string query = "?" + string.Join("&", array);
			return new UriBuilder(baseUri)
			{
				Query = query
			}.Uri;
		}
	}
}
