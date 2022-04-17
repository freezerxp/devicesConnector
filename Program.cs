using System.Text;
using System.Text.Json;
using devicesConnector;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () =>
{

    var cr = new ConfigRepository();
    var c = cr.Get();

    return c;
});



new KkmMapCreator().CrateMap(app);



app.Run();
