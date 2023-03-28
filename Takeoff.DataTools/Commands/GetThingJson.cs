using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.DataTools.Commands
{
    public class GetThingJsonOptions
    {
        [Option('i', "id", Required = true, HelpText = "The thing id to get.")] 
        public int Id { get; set; }

        [Option('o', "outputFile", Required = false, HelpText = "The full path of a file to output the formatted json to")]
        public string OutputFile { get; set; }
    }

    public class GetThingJsonCommand : BaseCommandWithOptions<GetThingJsonOptions>
    {
        protected override void Perform(GetThingJsonOptions arguments)
        {
            Console.WriteLine("****** Checking Memcached *******");
            var fromCache = CacheUtil.GetValueFromAppCache(Things.GetCacheKey(arguments.Id));
            if (fromCache == null)
            {
                Console.WriteLine("Thing was not found in the cache.");
            }
            else
            {
                Console.WriteLine("From cache:");
                Console.WriteLine(fromCache.ToString());
                Console.WriteLine("");
            }

            Console.WriteLine("****** Serializing using slow method *******");
            var thing = Things.Get(arguments.Id);
            Console.WriteLine(thing.SerializeToString());

            Console.WriteLine("****** Serializing using flast method *******");
            Console.WriteLine(thing.SerializeToString2());

            var results = GhettoPerfTest.GhettoTest(200,40,() =>
                                                 {
                                                     thing.SerializeToString();
                                                 },() =>
                                                 {
                                                     thing.SerializeToString2();
                                                 })
            ;
            Console.WriteLine("Slow " + results[0].TotalSeconds);
            Console.WriteLine("Fast " + results[1].TotalSeconds);

            if ( arguments.OutputFile.HasChars())
            {
                Console.WriteLine("Thing type {0}" + thing.GetType().FullName);
                var json = thing.SerializeJson();
                var text = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(arguments.OutputFile, text);
            }

        }
    }
}
