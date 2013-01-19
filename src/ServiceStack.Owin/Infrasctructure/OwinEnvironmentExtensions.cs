namespace ServiceStack.Owin.Infrasctructure
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    public static class OwinEnvironmentExtensions
	{
		public static T Get<T>(this IDictionary<string, object> environment, string key)
		{
			object value;
			return environment.TryGetValue(key, out value) && value is T ? (T) value : default(T);
		}

		public static void Set<T>(this IDictionary<string, object> environment, string key, T value)
		{
			environment[key] = value;
		}

		public static Uri GetRequestUri(this IDictionary<string, object> environment)
		{
			// From Owin spec 5.4 - URI Reconstruction Algorithm
			var uri = environment.Get<string>(OwinConstants.RequestSchemeKey) +
			                                  "://" +
			                                  environment.GetRequestHeader(OwinConstants.HostHeader) +
			                                  environment.Get<string>(OwinConstants.RequestPathBaseKey) +
			                                  environment.Get<string>(OwinConstants.RequestPathKey);
			if (!string.IsNullOrEmpty(environment.Get<string>(OwinConstants.RequestQueryStringKey)))
			{
				uri += "?" + environment.Get<string>(OwinConstants.RequestQueryStringKey);
			}
			return new Uri(uri);
		}

		public static T GetOwinEnvironmentValue<T>(this IDictionary<string, object> env, string name, T defaultValue = default(T))
		{
			object value;
			return env.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
		}

		public static IDictionary<string, string[]> GetOwinRequestHeaders(this IDictionary<string, object> env)
		{
			return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.RequestHeaders");
		}

		public static NameValueCollection GetRequestHeaders(this IDictionary<string, object> environment)
		{
			return GetHeaders(environment.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey));
		}

		public static NameValueCollection GetResponseHeaders(this IDictionary<string, object> environment)
		{
			return GetHeaders(environment.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey));
		}

		public static string GetRequestHeader(this IDictionary<string, object> environment, string key)
		{
			string[] value;
			return environment.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey).TryGetValue(key, out value) &&
			       value != null
				       ? string.Join(",", value.ToArray())
				       : null;
		}

		public static string GetResponseHeader(this IDictionary<string, object> environment, string key)
		{
			string[] value;
			return environment.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey).TryGetValue(key, out value) &&
				   value != null
					   ? string.Join(",", value.ToArray())
					   : null;
		}

		public static void SetRequestHeader(this IDictionary<string, object> environment, string key, string value)
		{
			var headers = environment.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey);
			SetHeader(headers, key, value);
		}

		public static void SetResponseHeader(this IDictionary<string, object> environment, string key, string value)
		{
			var headers = environment.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey);
			SetHeader(headers, key, value);
		}

		private static NameValueCollection GetHeaders(IEnumerable<KeyValuePair<string, string[]>> headerDictionary)
		{
			var headers = new NameValueCollection();
			foreach (var header in headerDictionary)
			{
				headers.Add(header.Key, string.Join(",", header.Value.ToArray()));
			}
			return headers;
		}

		private static void SetHeader(IDictionary<string, string[]> headers, string key, string value)
		{
			if (headers.ContainsKey(key))
			{
				headers.Add(key, new[] { value });
			}
			else
			{
				headers[key] = new[] { value };
			}
		}
	}
}