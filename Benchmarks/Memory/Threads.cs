// static class Threads
// {
//     public static void Test(int threadCount, bool noop = false)
//     {
//         var counters = new int[threadCount];
//         for (int i = 0; i < counters.Length; i++)
//         {
//             int index = i;
//             Console.WriteLine("thread" + index);
//             if (noop)
//             {
//                 new Thread(() =>
//                 {
//                     var test = new UI();
//                     var ctx = new UI<ViewModel>.HttpXContext(test);

//                     while (true)
//                     {
//                         test.NoOp();
//                         counters[index]++;
//                     }
//                 }).Start();
//             }
//             else
//             {
//                 new Thread(() =>
//                 {
//                     var ui = new UI();
//                     var ctx = new UI<ViewModel>.HttpXContext(ui);
//                     // ctx.Compose();
//                     Console.WriteLine("Warmup:");
//                     Console.WriteLine(ctx.ToString());

//                     while (true)
//                     {
//                         // ctx.Compose();
//                         counters[index]++;
//                     }
//                 }).Start();
//             }
//         }


//         new Thread(async () =>
//         {
//             while (true)
//             {
//                 long gc1 = GC.GetTotalAllocatedBytes();
//                 await Task.Delay(1000);
//                 long gc2 = GC.GetTotalAllocatedBytes();

//                 int perSecond = counters.Sum();
//                 Console.Write("Total: {0:n0} composes/s   {1:n0} bytes allocated    ", perSecond, gc2 - gc1);

//                 for (int i = 0; i < counters.Length; i++)
//                 {
//                     Console.Write($"{counters[i],10:n0}  ");
//                 }

//                 Console.WriteLine();

//                 counters = new int[counters.Length];
//             }
//         }).Start();
//     }
// }