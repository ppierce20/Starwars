using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarWarsPassengers.Models;
using StarWarsPassengers.Abstract;

namespace StarWarsPassengers
{
    public class StarWarsApiClient : CachedApiClient, IStarWarsApiClient
    {
        public StarWarsApiClient() : this("https://swapi.co/api")
        {
        }

        public StarWarsApiClient(string baseUrl) : base(baseUrl)
        {
        }

        /// <summary>
        /// this will call the base get with the people url. it will continue to get the next page
        /// until there are none left. after it gets a page it will filter the results to the predicate
        /// passed in then it will yield return them as it gets them.
        /// </summary>
        /// <param name="filter">predicate</param>
        /// <returns>yield returns people</returns>
        public IEnumerable<Person> GetPeople(Func<Person, bool> filter = null)
        {
            var next = "/people/";

            do
            {
                var result = base.Get<Response<List<Person>>>(next);
                next = result.Next;
                foreach (var person in result.Results.Where(filter ?? ((x) => true)))
                    yield return person;
            } while (next != null);
        }

        /// <summary>
        /// this will call the base get with the starships url. it will continue to get the next page
        /// until there are none left. after it gets a page it will filter the results to the predicate
        /// passed in then it will yield return them as it gets them.
        /// </summary>
        /// <param name="filter">predicate</param>
        /// <returns>yield returns people</returns>
        public IEnumerable<Starship> GetStarships(Func<Starship, bool> filter = null)
        {
            var next = "/starships/";

            do
            {
                var result = base.Get<Response<List<Starship>>>(next);
                next = result.Next;
                foreach (var ship in result.Results.Where(filter ?? ((x) => true)))
                    yield return ship;
            } while (next != null);
        }

        public Person GetPerson(string url)
        {
            return (GetPersonAsync(url).Result);
        }

        /// <summary>
        /// async method that will call the get to get a person
        /// </summary>
        /// <param name="url">this needs to include the id</param>
        /// <returns>Task of Person</returns>
        public async Task<Person> GetPersonAsync(string url)
        {
            return (await Task.Run(() => base.Get<Person>(url)));
        }

        public Starship GetStarship(string url)
        {
            return (GetStarshipAsync(url).Result);
        }

        /// <summary>
        /// async method that will call the get to get a starship
        /// </summary>
        /// <param name="url">this needs to include the id</param>
        /// <returns>Task of Starship</returns>
        public async Task<Starship> GetStarshipAsync(string url)
        {
            return (await Task.Run(() => base.Get<Starship>(url)));
        }
    }
}
