using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var wrapperWidth = 960;
var wrapperHeight = 720;
var cellSize = 10;
var centerX = wrapperWidth / 2d;
var centerY = wrapperHeight / 2d;

var angle = 0d;
var radius = 0d;

var tiles = new List<Point>();
var step = cellSize;

while (radius < Math.Min(wrapperWidth, wrapperHeight) / 2d)
{
    var x = centerX + Math.Cos(angle) * radius;
    var y = centerY + Math.Sin(angle) * radius;

    if (x >= 0 && x <= wrapperWidth - cellSize && y >= 0 && y <= wrapperHeight - cellSize)
    {
        tiles.Add(new(x, y));
    }

    angle += 0.2;
    radius += step * 0.015;
}

app.MapGet("/", ctx => $$"""
  <!DOCTYPE html>
  <html>
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0">
      <style>
      body {
        display: flex;
        justify-content: center;
        align-items: center;
        height: 100vh;
        background-color: #f0f0f0;
        margin: 0;
      }
      #wrapper {
        width: 960px;
        height: 720px;
        position: relative;
        background-color: white;
      }
      .tile {
        position: absolute;
        width: 10px;
        height: 10px;
        background-color: #333;
      }
      </style>
    </head>
    <body>
      <div id="root">
        {{ tiles.Select(t => Tile(t.X, t.Y)) }}
      </div>
    </body>
  </html>
  """);

app.Run();

Html Tile(double x, double y) => $"""
  <div 
    class="tile"
    style="left: {x:F2}px; top: {y:F2}px;">
  </div>
  """;

record Point(double X, double Y);