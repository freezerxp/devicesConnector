using System.Text;
using System.Text.Json;
using devicesConnector;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");




var p = new UrlMapCreator(app);
p.CrateMaps();


app.Run();
