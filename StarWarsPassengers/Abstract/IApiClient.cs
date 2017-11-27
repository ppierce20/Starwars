using System.Collections.Generic;

namespace StarWarsPassengers.Abstract
{
    interface IApiClient
    {
        T Get<T>(string url, Dictionary<string, object> parameters = null) where T : class, new();
    }
}
