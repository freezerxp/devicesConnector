using System.Text;
using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Commands;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.FiscalRegistrar;

public  class KkmMapCreator: IMapCreator
{

    public void CrateMap(WebApplication app)
    {
        app.MapPost("/kkm/getStatus", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmGetStatusCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            
            var a = kkmH.GetStatus();


            return Results.Ok(new Answer(Answer.Statuses.Ok)
            {
                Data = a
            });
        });

        app.MapPost("/kkm/openSession", async (HttpContext context) =>
        {

            var c = await GetCommand<KkmOpenSessionCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            kkmH.OpenSession(c.Cashier);


            return Results.Ok(new Answer(Answer.Statuses.Ok));
        });

        app.MapPost("/kkm/cashInOut", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmCashInOutCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            kkmH.CashInOut(c.Sum, c.Cashier);


            return Results.Ok(new Answer(Answer.Statuses.Ok));
        });

        app.MapPost("/kkm/getReport", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmGetReportCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            kkmH.GetReport(c.ReportType, c.Cashier);


            return Results.Ok(new Answer(Answer.Statuses.Ok));
        });
    }

    private static async Task<TCommand> GetCommand<TCommand>(HttpContext context) where TCommand : KkmCommand
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        var command = JsonSerializer.Deserialize<TCommand>(json);

        return command;

    
    }
}