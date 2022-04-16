using System.Text;
using System.Text.Json;
using devicesConnector;
using devicesConnector.FiscalRegistrar;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");



new KkmMapCreator().CrateMap(app);



app.Run();
