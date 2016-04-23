﻿using Microsoft.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitSample
{

    /*
     * Related documents: 
     * 
     * Best Practices in Asynchronous Programming
     * https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
     * 
     * Await, SynchronizationContext, and Console Apps
     * http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
     * 
     * */
    class Program
    {
        static void Main(string[] args)
        {
            AsyncExceptionTester asyncTester = new AsyncExceptionTester();


            /*
             * 1. RULE: NEVER compose a method the has a signature of 'async void'!
             * Use 'async Task' instead. 'async void' is intended to make async event
             * handlers possible.
             * 
             * If below code is uncommented the exception causes a program termination
             * because it goes up as unhandled exception to the SynchronizationContext.
             * */

            //try
            //{
            //    asyncTester.AsyncVoidExceptionsCannotBeCaught();
            //}
            //catch (Exception e)
            //{

            //    Console.WriteLine("Async Void Exception not caught here, too.");
            //}


            try
            {
                asyncTester.AsyncTaskExceptionCaught();
            }
            catch (Exception e)
            {

                Console.WriteLine("Async Task Exception not caught here. It is caught in invoked method.");
            }


            try
            {
                asyncTester.DoSomeAsyncTaskOnConsoleMain();
            }
            catch (Exception e)
            {

                Console.WriteLine("Async Task Exception caught here.");
            }


            SomeAsyncAction(/*async*/ (taskId) =>
                            {
                                //try
                                //{
                                    Console.WriteLine("ActionTask '{0}' - before started.", taskId);
                                    /*await*/ asyncTester.DoSomeAction(taskId);
                                    Console.WriteLine("ActionTask '{0}' - after started.", taskId);
                                //}
                                //catch (Exception e)
                                //{
                                //    // Catch Exception from 'finally' block in DoSomeAction.
                                //    Console.WriteLine(e.Message);
                                //    if (e.InnerException != null)
                                //    {
                                //        Console.WriteLine(e.InnerException.Message);
                                //    }
                                //}
                                
                            });

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Main Thread doing other work ...");
            }

            Console.WriteLine("Main Thread other work finished.");

            AsyncPump.Run(async () =>
            {
                await DemoAsync();
            });

            Console.ReadLine();
        }




        internal static void SomeAsyncAction(Action<int> action)
        {
            // invoke a few times async method
            for (int i = 0; i < 10; i++)
            {
                action(i);
            }
        }

        static async Task DemoAsync()
        {
            var d = new Dictionary<int, int>();
            for (int i = 0; i < 10000; i++)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int count;
                d[id] = d.TryGetValue(id, out count) ? count + 1 : 1;

                await Task.Yield();
            }
            foreach (var pair in d) Console.WriteLine(pair);
        }
    }
}
