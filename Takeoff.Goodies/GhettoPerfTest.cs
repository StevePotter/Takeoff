using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System
{
    public static class GhettoPerfTest
    {


        public static TimeSpan[] GhettoTest(int numberOfRounds, int iterationsPerTest, params Action[] tests)
        {
            //stores a list for every test that records time for every round.
            var resultsPerRound = tests.Select(t => new List<TimeSpan>()).ToArray();

            for (var round = 0; round < numberOfRounds; round++)
            {
                for (var ti = 0; ti < tests.Length; ti++)
                {
                    var test = tests[ti];
                    var timer = Stopwatch.StartNew();
                    for (var i = 0; i < iterationsPerTest; i++)
                    {
                        test();
                    }
                    timer.Stop();
                    resultsPerRound[ti].Add(timer.Elapsed);
                }
            }
            //return is average round, not average runtime of a single test
            return resultsPerRound.Select(times => TimeSpan.FromTicks(times.Sum(t => t.Ticks) / (long)times.Count)).ToArray();
        }
        
    }
}
