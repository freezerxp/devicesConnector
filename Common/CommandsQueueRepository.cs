using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Commands;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.Common;

/// <summary>
/// Репозиторий очереди команд
/// </summary>
public class CommandsQueueRepository
{
    /// <summary>
    /// Статичная очередь команд на выполнение
    /// </summary>
    private static Queue<CommandQueue> _commandsQueue = new();

    /// <summary>
    /// Все команды (для получения статусов по запросу)
    /// todo: перенос в БД
    /// </summary>
    public static List<CommandQueue> CommandsHistory = new();

    /// <summary>
    /// Объект очереди
    /// </summary>
    public class CommandQueue
    {
        /// <summary>
        /// Команда
        /// </summary>
        public JsonNode Command { get; set; }

        /// <summary>
        /// Текущий статус
        /// </summary>
        public Answer.Statuses Status { get; set; }

        /// <summary>
        /// Результат выполнения команды
        /// </summary>
        public JsonNode? Result { get; set; }
    }

    /// <summary>
    /// Добавить команду в очередь
    /// </summary>
    /// <param name="command">Команда</param>
    public void AddToQueue(JsonNode command)
    {
        if (CommandsHistory.Any(x =>
                x.Command.Deserialize<DeviceCommand>()?.CommandId == command.Deserialize<DeviceCommand>()?.CommandId))
        {
            throw new Exception("Команда с указанным ИД уже есть в очереди");
        }

        var cq = new CommandQueue
        {
            Command = command,
            Status = Answer.Statuses.Wait
        };

        _commandsQueue.Enqueue(cq);
        CommandsHistory.Add(cq);
    }

    /// <summary>
    /// Получаем результат выполнения команды
    /// </summary>
    /// <param name="commandId">Идентификатор команды</param>
    /// <returns></returns>
    public CommandQueue GetCommandState(string commandId)
    {
        return CommandsHistory.First(x => x.Command.Deserialize<DeviceCommand>()?.CommandId == commandId);
    }

    /// <summary>
    /// Инициализация обработки очереди
    /// </summary>
    public static void Init()
    {
        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    DoCommand();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        });
    }

    /// <summary>
    /// Выполнение команды
    /// </summary>
    private static void DoCommand()
    {
        if (_commandsQueue.Any() == false)
        {
            return;
        }

        var c = _commandsQueue.Dequeue();

        if (c.Status != Answer.Statuses.Wait)
        {
            return;
        }

        var d = GetDeviceById(c.Command.Deserialize<DeviceCommand>().DeviceId);

        if (d.Type == Enums.DeviceTypes.FiscalRegistrar)
        {
            var manager = new KkmCommandsManager();
            manager.Do(c);
        }
    }


    public static void SetResult(Action action, CommandQueue cq)
    {
        try
        {
            action();
            cq.Status = Answer.Statuses.Ok;
        }
        catch (Exception e)
        {
            cq.Status = Answer.Statuses.Error;
            cq.Result = JsonSerializer.SerializeToNode(new ErrorObject(e));
        }
    }

    public static void SetResult<T>(Func<T> func, CommandQueue cq)
    {
        try
        {
            var result = func.Invoke();
            cq.Status = Answer.Statuses.Ok;
            cq.Result = JsonSerializer.SerializeToNode(result);
        }
        catch (Exception e)
        {
            cq.Status = Answer.Statuses.Error;
            cq.Result = JsonSerializer.SerializeToNode(new ErrorObject(e));
        }



     

    }

    /// <summary>
    /// Получить устройство по ИД
    /// </summary>
    /// <param name="id">ИД устройства</param>
    /// <returns></returns>
    public static Device GetDeviceById(string id)
    {
        var cr = new ConfigRepository();
        var c = cr.Get();

        var d = c.Devices.FirstOrDefault(x => x.Id == id);


        return d;
    }
}