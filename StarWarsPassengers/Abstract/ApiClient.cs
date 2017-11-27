using RestSharp;
using System.Collections.Generic;

namespace StarWarsPassengers.Abstract
{
    public abstract class ApiClient : IApiClient
    {
        private RestClient _client;
        private string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _client = new RestClient(baseUrl);
        }

        /// <summary>
        /// this combines the multiple gets for the original starwarsapi into a single call. it will also work with being passes a full url
        /// like in the case of people linked from starships, starships linked from people, and the paging from a call to /people or /starships.
        /// this will also allow the adding of any other parameters through the parameters dictionary (incase you need something other than id or page)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual T Get<T>(string url, Dictionary<string, object> parameters = null) where T : class, new()
        {
            //TODO: need error handling and to validate the url.
            var apiResource = url;
            if (url.StartsWith(_baseUrl))
                apiResource = url.Substring(_baseUrl.Length);

            RestRequest request = new RestRequest(apiResource);

            if (parameters != null)
            {
                foreach(var kvp in parameters)
                {
                    request.AddParameter(kvp.Key, kvp.Value);
                }
            }

            var results = _client.Execute<T>(request);

            return results.Data;
        }
    }
}
