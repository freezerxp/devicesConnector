using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Configs;
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

        if (_commandsQueue.Any(x => x.Command.Deserialize<DeviceCommand>()?.Id == command.Deserialize<DeviceCommand>()?.Id))
        {
            throw new Exception("Команда с указанным ИД уже есть в очереди");
        }

        var cq = new CommandQueue();
        cq.Command = command;
        cq.Status = Answer.Statuses.Wait;

        _commandsQueue.Enqueue(cq);
        CommandsHistory.Add(cq);
    }

    public CommandQueue GetCommandState(string commandId)
    {

      return   CommandsHistory.First(x => x.Command["Id"]?.ToString() == commandId);
        
    
    }


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
                    Console.WriteLine(e);
                    
                }

            }


        });
    }

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
            DoKkmCommand(c);
        }

      
    }

    private static  void DoKkmCommand(CommandQueue cq)
    {
        var deviceCommand = cq.Command.Deserialize<DeviceCommand>();

        if (deviceCommand == null)
        {
            throw new NullReferenceException();
        }

        var hi = CommandsHistory.First(x => x.Command["Id"]?.ToString() == deviceCommand.Id);
        hi.Status = Answer.Statuses.Run;

        var commandType = (FiscalRegistrar.Objects.Enums.CommandTypes) deviceCommand.CommandType;

        var d = GetDeviceById(deviceCommand.DeviceId);

        var kkm = new FiscalRegistrarFacade(d);

        switch (commandType)
        {
            case FiscalRegistrar.Objects.Enums.CommandTypes.GetStatus:
                
                var status = kkm.GetStatus();


                
                hi.Result = JsonSerializer.SerializeToNode(status);
                hi.Status = Answer.Statuses.Ok;

                break;
            case FiscalRegistrar.Objects.Enums.CommandTypes.OpenSession:
                break;
            case FiscalRegistrar.Objects.Enums.CommandTypes.CashInOut:
                break;
            case FiscalRegistrar.Objects.Enums.CommandTypes.DoReport:
                break;
            case FiscalRegistrar.Objects.Enums.CommandTypes.PrintFiscalReceipt:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private static Device GetDeviceById(string id)
    {
        var cr = new ConfigRepository();
        var c = cr.Get();

        var d = c.Devices.FirstOrDefault(x => x.Id == id);


        return d;
    }
}