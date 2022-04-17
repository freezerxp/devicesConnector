using System.Text;
using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Commands;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.FiscalRegistrar;

public class KkmMapCreator : IMapCreator
{
    public void CrateMap(WebApplication app)
    {
        app.MapPost("/kkm/getStatus", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmGetStatusCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            return GetResult(() => kkmH.GetStatus());

        });

        app.MapPost("/kkm/openSession", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmOpenSessionCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            return GetResult(() => kkmH.OpenSession(c.Cashier));
        });

        app.MapPost("/kkm/cashInOut", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmCashInOutCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            return GetResult(() => kkmH.CashInOut(c.Sum, c.Cashier));
        });

        app.MapPost("/kkm/getReport", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmGetReportCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);


            return GetResult(() => kkmH.GetReport(c.ReportType, c.Cashier));
        });

        app.MapPost("/kkm/printFiscalReceipt", async (HttpContext context) =>
        {
            var c = await GetCommand<KkmPrintFiscalReceiptCommand>(context);

            var kkmH = new FiscalRegistrarFacade(c.Connection, c.KkmType);

            return GetResult(() => kkmH.PrintFiscalReceipt(c.ReceiptData));
        });
    }

    private static IResult GetResult<T>(Func<T> func)
    {
        T result;

        try
        {
             result = func.Invoke();


        }
        catch (Exception e)
        {
            return Results.Json(new Answer(Answer.Statuses.Error, e));
        }


        return Results.Ok(new Answer(Answer.Statuses.Ok, result));

    }

    private static IResult GetResult(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            return Results.Json(new Answer(Answer.Statuses.Error, e));
        }

        return Results.Ok(new Answer(Answer.Statuses.Ok, null));
    }

    private static async Task<TCommand> GetCommand<TCommand>(HttpContext context) where TCommand : KkmCommand
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        var command = JsonSerializer.Deserialize<TCommand>(json);

        return command;
    }
}