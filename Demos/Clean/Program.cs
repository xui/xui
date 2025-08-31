using System.Collections;
using System.Drawing;
using System.Security.Cryptography;
using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string name = "World";
int c = 16;
double d = 3.14;
List<string> names = ["one", "two", "three", "four", "five", "six", "seven"];
Color color = Color.Green;
bool b = true;
bool @checked = true;

app.MapWeb4("/list", () => $"""
    <div>
        {GetList()}
    </div>
    """);

Html GetList() => $"""
    <list>

        {names.Select(n => $"""
            <lr>
                <ld>{ Icons.Globe }</ld>
                <ld>Hello, { n }!</ld>
            </lr>
        """):zoom-fade}

        <button onclick={EditRandom}>
            Edit Random
        </button>

        <button onclick={Shuffle}>
            Shuffle
        </button>

        <button onclick={AddOne}>
            Add One
        </button>

        <button onclick={RemoveOne}>
            Remove One
        </button>
        
    </list>
    """;

void EditRandom()
{
    var index = Random.Shared.Next(names.Count);
    names[index] = names[index] + " &#128514;";
}

void Shuffle()
{
    names = names.OrderBy(n => Random.Shared.Next()).ToList();
}

void AddOne()
{
    names.Add(names.Count.ToString());
}

void RemoveOne()
{
    names.RemoveAt(names.Count - 1);
}

app.MapWeb4("/swiftui", () => $"""
    <column>
        { Icons.Globe }
        Hello, World!
    </column>
    """);

var window = app.MapWeb4("/app", () => $"""
    <!doctype app>
    <html>
        <head>
            <title>Neato</title>
        </head>
        <body>
            <button onmousedown={() => c++}>
                Clicks: {c}
            </button>
            {c:c} and {d:c}

            {$"<div>one {name} {c} four</div>"}
            {MyButton(text: "Nice to repeat you")}
            {MyButton(text: "Nice to repeat you")}

            <br />
            <br />
            {MyButton(text: "111")}{MyButton(text: "222")}{MyButton(text: "333")}{MyButton(text: "444")}
            <br />
            <br />
 
            <row> 
                { names.Select(n => NoCButton(text: n)) }
            </row>

            <section>
                <h2>Attribute stuffs</h2>
                <input type="number" value={c} oninput={e => c = e.Target.Value} /> {c}
                <br/>
                <input type="text" value={name} oninput={e => name = e.Target.Value} /> {name}
                <br/>
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b}
                <br/>
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b}
                <br/>
            </section>
            
            {(b
                ? (Html)$"<row>one {name}<br>{c} four</row>" 
                : (Html)$"<row>{color}</row>"
            ):zoom-fade}

            <row>
                {true switch {
                    _ when c < 5  => MyButton(text: "Neato"),
                    _ when c == 15 => NoCButton(name),
                    _ when c > 50 => YourButton(),
                    _ => NoneButton()
                }}
            </row>


            <br/><br/>

            <input type="color" value={color} oninput={e => color = e.Target.Value} />
            <span style={$"color: {color}; font-size: {c}pt;"}>{color:RGB}</span>
            <br/>

            <h2>Click on the buttons</h2>
            <div>
            outer div<br />
                <div>
                    middle div<br />
                    <div>
                        inner div<br />
                        <button>allow propagation</button><br />
                        <button id="stopPropagation">stop propagation</button><br />
                        <button id="stopImmediatePropagation">immediate stop propagation</button>
                    </div>
                </div>
            </div>
            
            {ComponentWithComponentInside()}

            and one more for that big time key-rewind
            {MyButton(text: "Nice to repeat you")}

        </body>
    </html>
    """);

Html MyButton(string text, Action<Event>? onClick = null) => $"""
    <button onclick={onClick ?? OnClick}>
        Hello {text}! Clicks: {c}
    </button>
    """;

Html YourButton(string? text = null) => $"""
    <button>
        Clicks: {c} - {text ?? "whatevs"}
    </button>
    """;

Html NoCButton(string text) => $"""
    
    <span>{text}, </span>
    """;

Html NoneButton() => $"""
    <span>None</span>
    """;

Html ComponentWithComponentInside() => $"""
    <div>
        {MyButton(text: "3rd level?")}
    </div>
""";

static void OnClick(Event.Mouse e)
{
    // c++;

    var x = e.X;
}

// window.OnClick = e => Console.WriteLine($"win0: window.OnClick 0: {"e"}");
// window.AddEventListener("afterprint", (Event e) => Console.WriteLine($"win1: afterprint: {"e"}"));
// window.AddEventListener("dblclick", () => Console.WriteLine("win2: dblclick:"));
// window.AddEventListener("click", e => d = e.X);
// window.OnClick = e => Console.WriteLine($"win4: window.OnClick 1: {"e"}");
// window.Document.OnSelectionChange = e => Console.WriteLine($"doc5: {e.Type}");
// window.Document.AddEventListener("click", e => Console.WriteLine($"doc6: document.onclick: {e.X}"));
// window.AddEventListener("mousemove", e => c = (int)e.X);

window.MapGet("/about", ctx =>
{
    name = "Reset";
    c = 0;
});

app.Run();








static class Icons
{
    public static Html Globe => $"""
        <svg style="width: 24px; height: 24px;" viewBox="0 0 63.9375 63.7812">
        <g>
        <rect height="63.7812" opacity="0" width="63.9375" x="0" y="0"/>
        <path fill="#0033ff" d="M31.875 62C40.5625 62 47.6562 49.1562 47.6562 31.9375C47.6562 14.625 40.5938 1.78125 31.875 1.78125C23.1562 1.78125 16.0938 14.625 16.0938 31.9375C16.0938 49.1562 23.1875 62 31.875 62ZM31.875 5.5625C38 5.5625 43.4688 17.75 43.4688 31.9375C43.4688 45.875 38 58.1875 31.875 58.1875C25.75 58.1875 20.2812 45.875 20.2812 31.9375C20.2812 17.75 25.75 5.5625 31.875 5.5625ZM29.8438 2.21875L29.8438 61.4062L33.9375 61.4062L33.9375 2.21875ZM31.875 43.625C22.25 43.625 13.7188 46.1562 9.34375 50.1875L12.5 52.7812C16.625 49.5 23.3438 47.7188 31.875 47.7188C40.4062 47.7188 47.125 49.5 51.25 52.7812L54.4062 50.1875C50.0312 46.1562 41.5 43.625 31.875 43.625ZM60.7188 29.8125L3.03125 29.8125L3.03125 33.9062L60.7188 33.9062ZM31.875 20.2812C41.5 20.2812 50.0312 17.75 54.4062 13.7188L51.25 11.125C47.125 14.375 40.4062 16.1875 31.875 16.1875C23.3438 16.1875 16.625 14.375 12.5 11.125L9.34375 13.7188C13.7188 17.75 22.25 20.2812 31.875 20.2812ZM31.875 63.75C49.4688 63.75 63.75 49.4688 63.75 31.875C63.75 14.2812 49.4688 0 31.875 0C14.2812 0 0 14.2812 0 31.875C0 49.4688 14.2812 63.75 31.875 63.75ZM31.875 59.5C16.625 59.5 4.25 47.125 4.25 31.875C4.25 16.625 16.625 4.25 31.875 4.25C47.125 4.25 59.5 16.625 59.5 31.875C59.5 47.125 47.125 59.5 31.875 59.5Z" fill="white" fill-opacity="0.85"/>
        </g>
        </svg>
        """;
}