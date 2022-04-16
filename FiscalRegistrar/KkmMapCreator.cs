using System.Text;
using System.Text.Json;
using devicesConnector.FiscalRegistrar.Commands;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.FiscalRegistrar;

public  class KkmMapCreator: IMapCreator
{
    public void CrateMap(WebApplication app)
    {
        app.MapPost("/kkm/getStatus", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var c = JsonSerializer.Deserialize<KkmGetStatusCommand>(json);

            var kkmH = new KkmHelper(c.Connection, c.KkmType);

            var a = kkmH.GetStatus();


            return Results.Ok(new Answer(Answer.Statuses.Ok)
            {
                Data = a
            });
        });

        app.MapPost("/kkm/openSession", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var c = JsonSerializer.Deserialize<KkmOpenSessionCommand>(json);

            var kkmH = new KkmHelper(c.Connection, c.KkmType);

            kkmH.OpenSession(c.Cashier);


            return Results.Ok(new Answer(Answer.Statuses.Ok));
        });

        app.MapPost("/kkm/cashInOut", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var c = JsonSerializer.Deserialize<KkmCashInOutCommand>(json);

            var kkmH = new KkmHelper(c.Connection, c.KkmType);

            kkmH.CashInOut(c.Sum, c.Cashier);


            return Results.Ok(new Answer(Answer.Statuses.Ok));
        });
    }
}