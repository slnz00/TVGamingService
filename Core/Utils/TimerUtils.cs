using System;
using System.Diagnostics;

namespace Core.Utils
{
    public static class TimerUtils
    {
        public static long MeasurePerformance(Action action)
        {
            var timer = new Stopwatch();

            timer.Start();
            action();
            timer.Stop();

            return timer.ElapsedMilliseconds;
        }
    }
}
