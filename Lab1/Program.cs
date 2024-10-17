using System;
using System.Threading.Tasks;
using System.Threading;

namespace Semaphore
{
    class Program
    {
        static int sharedResource = 0;
        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        static object lockObject = new object();
        static void Main(string[] args)
        {
            Task[] tasks = new Task[5];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => AccessSharedResource());
            }
            Task.WaitAll(tasks);
            Console.WriteLine("Value of shareed resources at the end: " + sharedResource);
        }
        static void AccessSharedResource()
        {
            for (int i = 0; i < 5; i++)
            {
                semaphore.Wait();
                Thread.Sleep(1000);
                lock (lockObject)
                {
                    sharedResource++;
                    Console.WriteLine("Thread " + Task.CurrentId + " incremented shared resource to " + sharedResource);
                }
                semaphore.Release();
            }
        }
    }
}
