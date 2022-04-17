using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Common;
using devicesConnector.Configs;

namespace devicesConnector;

public class MainMapCreator : IMapCreator
{
    /// <summary>
    /// Карта
    /// </summary>
    /// <param name="app"></param>
    public void CrateMap(WebApplication app)
    {
        app.MapGet("/", () =>
        {
            return "Hello!";
        });

        //настройки
        app.MapGet("/settings", () =>
        {

            var cr = new ConfigRepository();
            var c = cr.Get();

            return c;
        });

        //Добавление команды в очередь
        app.MapPost("/addCommand", async (HttpContext context) =>
        {

            var qr = new CommandsQueueRepository();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var jn = JsonNode.Parse(json);

            qr.AddToQueue(jn);

            var c = jn.Deserialize<DeviceCommand>();

            return Results.Ok(new Answer(Answer.Statuses.Wait, null)
                {
                    CommandId = c.CommandId,
                    DeviceId = c.DeviceId
                }
            );

        });

        //получение результатов выполнения команды
        app.MapGet("/getResult/{commandId}/",   (string commandId) =>
        {

            var qr = new CommandsQueueRepository();

         
            var ch = qr.GetCommandState(commandId);

            return Results.Ok(new Answer(ch.Status, ch.Result)
                {
                    CommandId = commandId,
                    DeviceId = ch.Command.Deserialize<DeviceCommand>()?.DeviceId
                }
            );

        });


    }
}