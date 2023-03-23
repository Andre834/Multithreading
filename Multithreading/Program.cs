using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;
namespace ClassLibraryNetStandard
{

    static class Program
    {
        internal static void Main()
        {
            WriteLine("Starting program...");
            Thread t = new Thread(PrintNumbersWithDelay);
            t.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));

            WriteLine("A thread has been aborted");
            Thread tp = new Thread(PrintNumbers);
            tp.Start();
            PrintNumbers();

            bool b = WaitFor.TryCallWithTimeout(
               OneSecondMethod,
               500.ToMilliseconds(), // timeout
                                     //  500ms => OneSecondMethod() gets Cancelled
                                     // 1500ms => OneSecondMethod() gets Executed
               out int result);
            Console.WriteLine($"OneSecondMethod() {(b ? "Executed" : "Cancelled")}");
        }
        static int OneSecondMethod(CancellationToken ct)
        {
            for (var i = 0; i < 10; i++)
            {
                Thread.Sleep(100.ToMilliseconds());
                // co-operative cancellation implies periodically check IsCancellationRequested 
                if (ct.IsCancellationRequested) { throw new TaskCanceledException(); }
            }
            return 123; // the result 
        }
        static TimeSpan ToMilliseconds(this int nbMilliseconds)
           => new TimeSpan(0, 0, 0, 0, nbMilliseconds);

        static void PrintNumbersWithDelay()
        {
            WriteLine("Starting...");
            for (int i = 1; i < 10; i++)
            {
                Sleep(TimeSpan.FromSeconds(2));
                WriteLine(i);
            }
        }

        static void PrintNumbers()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }
    }



    static class WaitFor
    {
        internal static bool TryCallWithTimeout<TResult>(
              Func<CancellationToken, TResult> proc,
              TimeSpan timeout,
              out TResult result)
        {
            // Request cancellation after a duration of 'timeout'
            var cts = new CancellationTokenSource(timeout);
            try
            {
                result = proc(cts.Token);
                return true;
            }
            catch (TaskCanceledException) { }
            finally { cts.Dispose(); }
            result = default;
            return false;
        }
    }
}