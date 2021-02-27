using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Timer
    {
        private static Dictionary<string, System.Diagnostics.Stopwatch> timers = new Dictionary<string, System.Diagnostics.Stopwatch>();
        //private static System.Diagnostics.Stopwatch stopwatch;
        //private static string timerInfo;
        //public Timer(string info)
        //{
        //    stopwatch = System.Diagnostics.Stopwatch.StartNew();
        //    timerInfo = info;
        //}

        public static void StartTimer(string name)
        {
            if(!timers.ContainsKey(name))
            {
                timers.Add(name, System.Diagnostics.Stopwatch.StartNew());
            }

            timers[name].Start();
        }

        public static void StopTimer(string name)
        {
            if(!timers.ContainsKey(name))
            {
                Debug.Log("No timer with the name: " + name + " exists");
                return;
            }
            timers[name].Stop();
        }

        public static void PrintTimer(string name)
        {
            if(!timers.ContainsKey(name))
            {
                Debug.Log("No timer with the name: " + name + " exists");
                return;
            }
            
            System.TimeSpan timeTaken = timers[name].Elapsed;
            Debug.Log("Time taken for " + name + ": " + timeTaken.ToString(@"m\:ss\.fff"));

            timers.Remove(name);
        }
    }
}

