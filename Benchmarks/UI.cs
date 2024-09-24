// using BenchmarkDotNet;
// using BenchmarkDotNet.Attributes;
// using BenchmarkDotNet.Running;

// [ShortRunJob]
// [MemoryDiagnoser]
// public class UI : UI<ViewModel>
// {
//     HttpXContext ctx;
//     int c = 15;
//     string s = "name";
//     static DateTime dt = DateTime.Now;

//     public UI() : base()
//     {
//         ctx = new(this);
//     }

//     [Benchmark]
//     [IterationCount(10)]
//     public void Test1()
//     {
//         // ctx.Compose();
//     }

//     protected override Html MainLayout(ViewModel vm)
//     {
//         return $"""
//             123{c}456{dt}789{c}0123

//             {new Component2(s)}
            
//             {dt:O} {c} {c:x} {c} {c} {c} {c}
//         """;
//     }

//     Html GetComponent1(string name)
//     {
//         return $"<p>I am {name} a component</p>";
//     }

//     [Benchmark]
//     [IterationCount(10)]
//     public Placebo Placebo()
//     {
//         return $"""
//             123{c}456{dt}789{c}0123

//             {GetPlaceboComponent(s)}

//             {dt:O} {c} {c:x} {c} {c} {c} {c}
//         """;
//     }

//     Placebo GetPlaceboComponent(string name)
//     {
//         return $"<p>I am {s} a component</p>";
//     }
// }

// readonly record struct Component2(string Name) : IView
// {
//     public Html Render() => $"<p>I am {Name} a component</p>";
// }

// class Wat
// {
//     void DoIt()
//     {
//         Component2 c = new();
//         DoIt2(ref c);
//     }

//     void DoIt2(ref Component2 component2)
//     {

//     }
// }