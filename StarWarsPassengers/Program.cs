using StarWarsPassengers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using StarWarsPassengers.Abstract;

namespace StarWarsPassengers
{
    class Program
    {
        static void Main(string[] args)
        {
            // Story: The intergalactic logistics team needs a program that can generate a list of Starship and Pilot combinations that can transport a given number of passengers.
            // For example, if I have 3000 passengers I need to know which starships can fit 3000 passengers and which pilots can pilot them.
            // Write a program or function that accepts a number passengers as input and outputs a string "{Starship} - {Pilot}" for each suitable starship and pilot combination.


            /* Tried a few different ways to get the data. I originally thought that \starships would get me all of them and then I could filter and then load all of the pilots
             * at the same time...but because of the paging when calling \starships I only get 10 of them at a time. So I handle the paging in the apiclient and this means that
             * the best I can do is load a page of starships, then filter to starships that meet my criteria and then asynchronous load all of the pilots for that starship
            */

            var passengers = 1;

            // **** Version 0 ****
            // this version calls version 1 but then I use links foreach to write the results to the console. however, to do this I have to convert the IEnumerable to a List this
            // will block and wait for all of the results (kill the benefits of yield return) before writing anything.

            //Version0(passengers); //to see this just uncomment
            // **** Version 0 ****


            // **** Version 1 ****
            // this version calls version 1 and will load 1 pilot at a time

            Version1(passengers);
            // **** Version 1 ****


            // **** Version 2 ****
            //this version calls version 2 and will load all pilots from a starship at the same time (this should be faster...and usually is, however, a lot of the results have
            //very few pilots and there is overhead with the tasks. if the data had more pilots per starship then this version should really pull ahead.

            var client = new StarWarsApiClient();
            Version2(passengers, client);
            // **** Version 2 ****


            // **** Version 2a ****
            //just wanted to test the effects of running from the cache instead of calling the api. the effect is dramatic. orders of magnitude better (not unexpected).

            //Version2a(passengers, client); //to see this just uncomment
            // **** Version 2a ****

            Console.ReadKey();
        }

        /// <summary>
        /// version 1 of the function. This effectively undoes all of my work to make everything as async as possible.
        /// for each of the starships it will lookup each of those pilots 1 at a time and it will block and wait for
        /// each pilot to complete before moving on.
        /// </summary>
        /// <param name="client">inversion of control</param>
        /// <param name="passengers"></param>
        /// <param name="displayFunc">this will allow you to specify how to display the results</param>
        /// <returns></returns>
        public static IEnumerable<string> GetStarshipPilotCombosByPassengersVersion1(IStarWarsApiClient client, int passengers, Func<Starship, Person, string> displayFunc = null)
        {
            int parsedPassengers = 0;
            var starships = client.GetStarships(x => Int32.TryParse(x.Passengers, out parsedPassengers) && parsedPassengers >= passengers); //getstarships will handle the paging for me. it will yield return each pages results then move on to the next page

            foreach (var starship in starships)
            {
                foreach (var pilot in starship.Pilots)
                {
                    var pilotResult = (client.GetPersonAsync(pilot)).Result;
                    yield return (displayFunc != null ? displayFunc(starship, pilotResult) : $"{starship.Name} - {pilotResult.Name}"); //the .Result is going to block and wait on this pilot to finish loading before it yield returns and moves on to the next pilot.
                }
            }
        }

        /// <summary>
        /// version 2 of the function. For each starship that meets the criteria this will start a new async task to
        /// get that pilots data. When all of the tasks complete for the given starship then it will yield return the
        /// required string output then it will move to the next sharship.
        /// </summary>
        /// <param name="client">inversion of control</param>
        /// <param name="passengers"></param>
        /// <param name="displayFunc">this will allow you to specify how to display the results</param>
        /// <returns></returns>
        public static IEnumerable<string> GetStarshipPilotCombosByPassengersVersion2(IStarWarsApiClient client, int passengers, Func<Starship, Person, string> displayFunc = null)
        {
            int parsedPassengers = 0;
            var starships = client.GetStarships(x => Int32.TryParse(x.Passengers, out parsedPassengers) && parsedPassengers >= passengers); //getstarships will handle the paging for me. it will yield return each pages results then move on to the next page
            var pilotTasks = new List<Task<Person>>();

            foreach (var starship in starships)
            {
                pilotTasks.Clear();

                foreach (var pilot in starship.Pilots)
                {
                    pilotTasks.Add(Task.Run(() => client.GetPersonAsync(pilot))); //here we are going to fire off a task for each pilot for this starship
                }

                Task.WaitAll(pilotTasks.ToArray()); //when we have data on all of the pilots for this starship

                foreach (var task in pilotTasks)
                    yield return (displayFunc != null ? displayFunc(starship, task.Result) : $"{starship.Name} - {task.Result.Name}"); //yield return the pilots for this starship
            }
        }

        #region Helper Methods

        /// <summary>
        /// just wanted to test the effects of running from the cache instead of calling the api. the effect is dramatic. orders of magnitude better (not unexpected).
        /// </summary>
        /// <param name="passengers"></param>
        /// <param name="client"></param>
        private static void Version2a(int passengers, StarWarsApiClient client)
        {
            var timeStartV2a = new TimeSpan(DateTime.Now.Ticks).Ticks;
            foreach (var starshipPilotCombo in GetStarshipPilotCombosByPassengersVersion2(client, passengers)) //with this call client is old and the results will be from cache
            {
                Console.WriteLine(starshipPilotCombo);
            }

            Console.WriteLine(new TimeSpan(DateTime.Now.Ticks).Ticks - timeStartV2a);
        }

        /// <summary>
        /// this version calls version 2 and will load all pilots from a starship at the same time (this should be faster...and usually is, however, a lot of the results have
        /// very few pilots and there is overhead with the tasks. if the data had more pilots per starship then this version should really pull ahead.
        /// </summary>
        /// <param name="passengers"></param>
        /// <param name="client"></param>
        private static void Version2(int passengers, StarWarsApiClient client)
        {
            var timeStartV2 = new TimeSpan(DateTime.Now.Ticks).Ticks;
            foreach (var starshipPilotCombo in GetStarshipPilotCombosByPassengersVersion2(client, passengers, (starship, person) => $"Version2 - {starship.Name} - {person.Name}")) //with this call client is new and the results will be real
            {
                Console.WriteLine(starshipPilotCombo);
            }

            Console.WriteLine(new TimeSpan(DateTime.Now.Ticks).Ticks - timeStartV2);
        }

        /// <summary>
        /// this version calls version 1 and will load 1 pilot at a time
        /// </summary>
        /// <param name="passengers"></param>
        private static void Version1(int passengers)
        {
            var timeStartV1 = new TimeSpan(DateTime.Now.Ticks).Ticks;
            foreach (var starshipPilotCombo in GetStarshipPilotCombosByPassengersVersion1(new StarWarsApiClient(), passengers)) //i am newing up StarWarsApiClient because if i used the same one then the 2nd call would used cached results and have an unfair advantage...but normally I would want the performance boost
            {
                Console.WriteLine(starshipPilotCombo);
            }

            Console.WriteLine(new TimeSpan(DateTime.Now.Ticks).Ticks - timeStartV1);
        }

        /// <summary>
        /// this version calls version 1 but then I use links foreach to write the results to the console. however, to do this I have to convert the IEnumerable to a List this
        /// will block and wait for all of the results (kill the benefits of yield return) before writing anything.
        /// </summary>
        /// <param name="passengers"></param>
        private static void Version0(int passengers)
        {
            var timeStartV0 = new TimeSpan(DateTime.Now.Ticks).Ticks;
            GetStarshipPilotCombosByPassengersVersion1(new StarWarsApiClient(), passengers).ToList().ForEach(x => Console.WriteLine(x));
            Console.WriteLine(new TimeSpan(DateTime.Now.Ticks).Ticks - timeStartV0);
        }

        #endregion
    }
}
