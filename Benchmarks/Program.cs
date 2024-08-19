﻿global using Xui.Web;
global using Xui.Web.HttpX;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

if (args.Length == 0)
{
    Console.WriteLine("Which test?\n  benchmarkdotnet\n  threads n");
    return;
}

switch (args[0])
{
    case "benchmarkdotnet":
        BenchmarkRunner.Run<UI>();
        break;
    case "threads":
        int threads = 1;
        bool placebo = false;
        if (args.Length > 1)
        {
            threads = int.Parse(args[1]);
            placebo = args[^1] == "placebo";
        }
        Threads(threads, placebo);
        break;
    default:
        throw new NotSupportedException("");
}

void Threads(int threadCount, bool placebo = false)
{
    var counters = new int[threadCount];
    for (int i = 0; i < counters.Length; i++)
    {
        int index = i;
        Console.WriteLine("thread" + index);
        if (placebo)
        {
            new Thread(() =>
            {
                var test = new UI();
                var ctx = new UI<ViewModel>.Context(test);

                while (true)
                {
                    test.Placebo();
                    counters[index]++;
                }
            }).Start();
        }
        else
        {
            new Thread(() =>
            {
                var ui = new UI();
                var ctx = new UI<ViewModel>.Context(ui);
                ctx.Compose();
                Console.WriteLine("Warmup:");
                Console.WriteLine(ctx.ToString());

                while (true)
                {
                    ctx.Compose();
                    counters[index]++;
                }
            }).Start();
        }
    }


    new Thread(async () =>
    {
        while (true)
        {
            long gc1 = GC.GetTotalAllocatedBytes();
            await Task.Delay(1000);
            long gc2 = GC.GetTotalAllocatedBytes();

            int perSecond = counters.Sum();
            Console.Write("Total: {0:n0} composes/s   {1:n0} bytes allocated    ", perSecond, gc2 - gc1);

            for (int i = 0; i < counters.Length; i++)
            {
                Console.Write($"{counters[i],10:n0}  ");
            }

            Console.WriteLine();

            counters = new int[counters.Length];
        }
    }).Start();
}
