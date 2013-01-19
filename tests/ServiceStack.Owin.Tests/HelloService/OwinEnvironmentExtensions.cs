namespace ServiceStack.Owin.Tests.HelloService
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Helper extension methods to work with the OWIN environment dictionary
    /// </summary>
    public static class OwinEnvironmentExtensions
    {
        public static T Get<T>(this IDictionary<string, object> environment, string key)
        {
            object value;
            return environment.TryGetValue(key, out value) && value is T ? (T) value : default(T);
        }

        public static T Get<T>(this IDictionary<string, object> dictionary, string key, Func<T> createValue)
        {
            var value = Get<T>(dictionary, key);
            if (Equals(value, default(T)))
            {
                value = createValue();
                dictionary[key] = value;
            }
            return value;
        }
    }
}