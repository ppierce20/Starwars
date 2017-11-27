using System.Collections.Generic;
using Newtonsoft.Json;

namespace StarWarsPassengers.Abstract
{
    public class CachedApiClient : ApiClient
    {
        /// <summary>
        /// this dictionary will hold results from a call. the key is the url + parameters. so if we
        /// have already seen this exact request then we will just return those results.
        /// </summary>
        private Dictionary<string, object> cache;

        public CachedApiClient(string baseUrl) : base(baseUrl)
        {
            cache = new Dictionary<string, object>();
        }

        /// <summary>
        /// this only really adds checking the dictionary for this exact call to the api and return the cached results...
        /// or if this is a unique call then stash the results in the dictionary after it is finished.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override T Get<T>(string url, Dictionary<string, object> parameters = null)
        {
            var parametersSerialized = string.Empty;
            if (parameters != null)
                parametersSerialized = JsonConvert.SerializeObject(parameters);

            var key = $"{url}{parametersSerialized}";

            if (cache.ContainsKey(key))
                return (cache[key] as T);

            var result = base.Get<T>(url, parameters);

            cache.Add(key, result);

            return result;
        }
    }
}
