using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class Timer
    {
        private System.Diagnostics.Stopwatch stopwatch;
        private string timerInfo;
        public Timer(string info)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            timerInfo = info;
        }

        public void StartTimer()
        {
            stopwatch.Start();
        }

        public void StopTimer()
        {
            stopwatch.Stop();
        }

        public void PrintTimer()
        {
            System.TimeSpan timeTaken = stopwatch.Elapsed;
            Debug.Log("Time taken for " + timerInfo + ": " + timeTaken.ToString(@"m\:ss\.fff"));
        }
    }
}

