using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarWarsPassengers.Models;

namespace StarWarsPassengers.Abstract
{
    interface IStarWarsApiClient
    {
        IEnumerable<Person> GetPeople(Func<Person, bool> filter = null);
        IEnumerable<Starship> GetStarships(Func<Starship, bool> filter = null);
        Person GetPerson(string url);
        Task<Person> GetPersonAsync(string url);
        Starship GetStarship(string url);
        Task<Starship> GetStarshipAsync(string url);
    }
}
