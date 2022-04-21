using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.Helpers;

namespace devicesConnector;

public class MainMapCreator : IMapCreator
{
    /// <summary>
    /// Карта
    /// </summary>
    /// <param name="app"></param>
    public void CrateMap(WebApplication app)
    {
        //домашняя
        app.MapGet("/", () => "Hello!");

        //настройки
        app.MapGet("/settings", GetConfig);

        //Добавление команды в очередь
        app.MapPost("/addCommand", async (HttpContext context) => { return await AddCommand(context); });

        //получение результатов выполнения команды
        app.MapGet("/getResult/{commandId}/", GetResult);
    }

    /// <summary>
    /// Получение настроек 
    /// </summary>
    /// <returns></returns>
    private static object GetConfig()
    {
        try
        {
            var cr = new ConfigRepository();
            var c = cr.Get();

            return c;
        }
        catch (Exception e)
        {
            return Results.BadRequest(new Answer(Answer.Statuses.Error, new ErrorObject(e)));

        }
    }

    /// <summary>
    /// Добавление команды в очередь
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task<IResult> AddCommand(HttpContext context)
    {
        try
        {
            var qr = new CommandsQueueRepository();

            //чтение данных из запроса
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var jn = JsonNode.Parse(json);
            var c = jn.Deserialize<DeviceCommand>();

            //присваиваю новый ИД команды, если небыл указан
            if (jn["CommandId"] == null)
            {
                var commandId = Guid.NewGuid().ToString();
                jn["CommandId"] = commandId;
                c.CommandId = commandId;
            }

          

            qr.AddToQueue(jn);

            var status = Answer.Statuses.Wait;


            if (c.WithOutQueue) //как бы без очереди, сразу возвращаем результат
            {
              

                do
                {
                    status = qr.GetCommandState(c.CommandId).Status;
                    Thread.Sleep(100);
                } while (status.IsEither(Answer.Statuses.Wait, Answer.Statuses.Run));

                return GetResult(c.CommandId);
            }

            //все хорошо
            return Results.Ok(new Answer(status, null)
                {
                    CommandId = c.CommandId,
                    DeviceId = c.DeviceId
                }
            );
        }
        catch (Exception e)
        {
            return Results.BadRequest(new Answer(Answer.Statuses.Error, new ErrorObject(e)));
        }
    }

    /// <summary>
    /// Получить результат выполнения команды по ИД
    /// </summary>
    /// <param name="commandId">ИД команды из очереди</param>
    /// <returns></returns>
    private static IResult GetResult(string commandId)
    {
        try
        {
            var qr = new CommandsQueueRepository();

            var ch = qr.GetCommandState(commandId);

            return Results.Ok(new Answer(ch.Status, ch.Result)
                {
                    CommandId = commandId,
                    DeviceId = ch.Command.Deserialize<DeviceCommand>()?.DeviceId
                }
            );
        }
        catch (Exception e)
        {
            return Results.BadRequest(new Answer(Answer.Statuses.Error, new ErrorObject(e)));
        }
    }
}