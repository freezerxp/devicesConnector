using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Devices;
using static devicesConnector.Common.CommandsQueueRepository;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Commands;

public class KkmCommandsManager: ICommandManager
{
    /// <summary>
    /// Выполнение команды для ККМ
    /// </summary>
    /// <param name="cq">Запись из очереди команд</param>
    public void Do(CommandQueue cq)
    {
        var deviceCommand = cq.Command.Deserialize<DeviceCommand>();

        if (deviceCommand == null)
        {
            throw new NullReferenceException();
        }

        var hi = CommandsHistory.First(x => x.Command.Deserialize<DeviceCommand>()?.CommandId == deviceCommand.CommandId);
        hi.Status = Answer.Statuses.Run;

        var commandType = (Enums.CommandTypes) deviceCommand.CommandType;

        var d = GetDeviceById(deviceCommand.DeviceId);

        var kkm = new FiscalRegistrarFacade(d);

        switch (commandType)
        {
            case Enums.CommandTypes.GetStatus:


                GetStatus(kkm, hi);


                break;
            case Enums.CommandTypes.OpenSession:
                break;
            case Enums.CommandTypes.CashInOut:
                break;
            case Enums.CommandTypes.DoReport:
                DoReport( kkm, hi);

                break;
            case Enums.CommandTypes.PrintFiscalReceipt:
                break;
           
              
        }



      






    }

    private static void DoReport( FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var drc = cq.Command.Deserialize<KkmGetReportCommand>();


        var action = () => kkm.GetReport(drc.ReportType, drc.Cashier);

        SetResult(action, cq);
    }

    private static void GetStatus(FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var func = () => kkm.GetStatus();
        SetResult(func, cq);
    }
}