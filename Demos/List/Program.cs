global using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var name = "World";
List<string> names = ["one", "two", "three", "four", "five", "six", "seven"];

app.MapWeb4("/", () => $"""
    <list>
        {names.Select(str => $"""
            <lr>
                <ld>{ Icons.Globe }</ld>
                <ld>Hello { name }, { str }!</ld>
            </lr>
        """):zoom-fade}
        <button onclick={ EditRandom }>Edit Random</button>
        <button onclick={ AddOne }>Add One</button>
        <button onclick={ RemoveOne }>Remove One</button>
        <button onclick={ Move1 }>Move 1</button>
        <button onclick={ Swap2 }>Swap 2</button>
        <button onclick={ Shuffle }>Shuffle</button>
        <button onclick={ DoThingAsync }>Async</button>
    </list>
""");

void EditRandom(Event e)
{
    var index = Random.Shared.Next(names.Count);
    names[index] += " 😀";
}

void AddOne()
{
    names.Add(names.Count.ToString());
}

void RemoveOne()
{
    names.RemoveAt(names.Count - 1);
}

void Move1()
{
    var i1 = Random.Shared.Next(names.Count);
    var i2 = Random.Shared.Next(names.Count);
    var item = names[i1];
    names.RemoveAt(i1);
    names.Insert(i2, item);
}

void Swap2()
{
    var i1 = Random.Shared.Next(names.Count);
    var i2 = Random.Shared.Next(names.Count);
    (names[i1], names[i2]) = (names[i2], names[i1]);
}

void Shuffle()
{
    names = names.OrderBy(n => Random.Shared.Next()).ToList();
}

async Task DoThingAsync(Event.Subsets.View e)
{
    var window = e.View;
    var console = window.Console;

    console.Log("one");
    console.Log("two");
    await Task.Delay(2000);
    
    console.Log("three");
    console.Log("four");
    await Task.Delay(2000);
    
    console.Log("five");
    console.Log("six");
    int i = 1;
    i--;
    i /= i;
    
    window.Alert("Neato");
    console.Log("seven");
}

app.Run();