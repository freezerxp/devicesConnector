
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Common;
using devicesConnector.Configs;


namespace devicesConnector.FiscalRegistrar;

public class KkmMapCreator : IMapCreator
{
    public void CrateMap(WebApplication app)
    {

        app.MapPost("/addCommand", async (HttpContext context) =>
        {
            //var c = await GetCommand(context);

            var qr = new CommandsQueueRepository();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var jn = JsonNode.Parse(json);

            qr.AddToQueue(jn);

            var c = jn.Deserialize<DeviceCommand>();

            return Results.Ok(new Answer(Answer.Statuses.Wait, null)
                {
                    CommandId = c.Id,
                    DeviceId = c.DeviceId
                }
           );

        });

        app.MapPost("/getCommand", async (HttpContext context) =>
        {

            var qr = new CommandsQueueRepository();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var jn = JsonNode.Parse(json);


            var c = jn.Deserialize<DeviceCommand>();


            var ch = qr.GetCommandState(c.Id);

            return Results.Ok(new Answer(ch.Status, ch.Result)
                {
                    CommandId = c.Id,
                    DeviceId = c.DeviceId
                }
            );

        });




        //app.MapPost("/kkm/getStatus", async (HttpContext context) =>
        //{
        //    var c = await GetCommand<KkmGetStatusCommand>(context);

        //    var qr = new CommandsQueueRepository();

        //    qr.AddToQueue(c);

        //    return Results.Ok(new Answer(Answer.Statuses.Wait, null)
        //    {
        //        CommandId = c.Id,
        //        DeviceId = c.DeviceId
        //    });

        //    //var d = GetDeviceById(c.DeviceId);

        //    //var kkmH = new FiscalRegistrarFacade(d);

        //    //return GetResult(() => kkmH.GetStatus());

        //});

        //app.MapPost("/kkm/openSession", async (HttpContext context) =>
        //{
        //    var c = await GetCommand<KkmOpenSessionCommand>(context);

        //    var d = GetDeviceById(c.DeviceId);

        //    var kkmH = new FiscalRegistrarFacade(d);

        //    return GetResult(() => kkmH.OpenSession(c.Cashier));
        //});

        //app.MapPost("/kkm/cashInOut", async (HttpContext context) =>
        //{
        //    var c = await GetCommand<KkmCashInOutCommand>(context);

        //    var d = GetDeviceById(c.DeviceId);


        //    var kkmH = new FiscalRegistrarFacade(d);

        //    return GetResult(() => kkmH.CashInOut(c.Sum, c.Cashier));
        //});

        //app.MapPost("/kkm/getReport", async (HttpContext context) =>
        //{
        //    var c = await GetCommand<KkmGetReportCommand>(context);

        //    var d = GetDeviceById(c.DeviceId);

        //    var kkmH = new FiscalRegistrarFacade(d);


        //    return GetResult(() => kkmH.GetReport(c.ReportType, c.Cashier));
        //});

        //app.MapPost("/kkm/printFiscalReceipt", async (HttpContext context) =>
        //{
        //    var c = await GetCommand<KkmPrintFiscalReceiptCommand>(context);

        //    var d = GetDeviceById(c.DeviceId);

        //    var kkmH = new FiscalRegistrarFacade(d);

        //    return GetResult(() => kkmH.PrintFiscalReceipt(c.ReceiptData));
        //});
    }

    private IResult GetWaitResult(DeviceCommand command)
    {
        return Results.Ok(new Answer(Answer.Statuses.Wait, command)
        {
            CommandId = command.Id,
            DeviceId = command.DeviceId
        });
    }

    private Device GetDeviceById(string id)
    {
        var cr = new ConfigRepository();
        var c = cr.Get();

        var d = c.Devices.FirstOrDefault(x => x.Id == id);


        return d;
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
            var eo = new ErrorObject(e);
            return Results.Json(new Answer(Answer.Statuses.Error, eo));
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
            var eo = new ErrorObject(e);

            return Results.BadRequest(new Answer(Answer.Statuses.Error, eo));
        }

        return Results.Ok(new Answer(Answer.Statuses.Ok, null));
    }



    private static async Task<DeviceCommand> GetCommand(HttpContext context) 
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        var command = JsonSerializer.Deserialize<DeviceCommand>(json);

        return command;
    }
}