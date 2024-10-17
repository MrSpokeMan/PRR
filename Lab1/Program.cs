using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using System.Diagnostics;

namespace Semaphore
{
    class Program
    {
        static int sharedResource = 0;
        static SemaphoreSlim semaphoreLoad = new SemaphoreSlim(2, 2);
        static SemaphoreSlim semaphoreRelease = new SemaphoreSlim(1, 1);
        static object mineLockObject = new object();
        static object printLockObject = new object();
        static object releaseLockObject = new object();
        static int resourceSize = 2000;
        static int cartSize = 200;
        static int timeToDig = 1;
        static int timeToRelease = 1;
        static int timeToTravel = 1000;
        enum MinerStatus { Digging, Transporting, Unloading, Waiting }

        static void Main(string[] args)
        {
            long firstResult = 0;
            List<string> list = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                Task[] tasks = new Task[i];
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int j = 0; j < i; j++)
                {
                    tasks[j] = Task.Run(() => AccessSharedResource());
                }
                Task.WaitAll(tasks);
                long time = sw.ElapsedMilliseconds;
                sw.Stop();
                if (i == 1)
                {
                    firstResult = time;
                }
                float acceleration = (float)firstResult / (float)time;
                float effiecency = (float)acceleration / (float)i;
                //store data for later

                list.Add($"Miners: {i}, time: {sw}, acceleration {acceleration}, effiecency {effiecency}");
                // clear cache and wait for next test
                sharedResource = 0;
                resourceSize = 2000;
                Thread.Sleep(1000);
                Console.Clear();
            }
            Console.Clear();
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        static void PrintStatus(int? id, Semaphore.Program.MinerStatus status) {
            int pos = 3 + (id ?? 0);

            Console.SetCursorPosition(0, pos);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, pos);
            Console.WriteLine("Górnik " + id + ": " + status);

            Console.SetCursorPosition(0, 0);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(
                "Stan złoża: " + resourceSize + " jednostek węgla\n" +
                "Stan magazynu: " + sharedResource + " jednostek węgla\n"
            );


        }

        static void AccessSharedResource()
        {
            int privateCartSize = 0;
            while(resourceSize > 0)
            {
                lock (printLockObject)
                {
                    PrintStatus(Task.CurrentId, MinerStatus.Waiting);
                }
                semaphoreLoad.Wait();
                lock (printLockObject)
                {
                    PrintStatus(Task.CurrentId, MinerStatus.Digging);
                }
                for (int i = 0; i < cartSize; i++)
                {
                    Thread.Sleep(timeToDig);
                    lock (mineLockObject)
                    {
                        if (resourceSize > 0)
                        {
                            resourceSize--;
                            privateCartSize++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                semaphoreLoad.Release();
                lock (printLockObject)
                {
                    PrintStatus(Task.CurrentId, MinerStatus.Transporting);
                }
                Thread.Sleep(timeToTravel);
                semaphoreRelease.Wait();
                lock (printLockObject)
                {
                    PrintStatus(Task.CurrentId, MinerStatus.Unloading);
                }
                while (privateCartSize > 0)
                {
                    Thread.Sleep(timeToRelease);
                    lock (releaseLockObject)
                    {
                        sharedResource++;
                        privateCartSize--;
                    }
                }
                semaphoreRelease.Release();
                lock (printLockObject)
                {
                    PrintStatus(Task.CurrentId, MinerStatus.Transporting);
                }
                Thread.Sleep(timeToTravel);
            }
        }
    }
}
