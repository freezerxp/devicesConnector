using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Devices;
using static devicesConnector.Common.CommandsQueueRepository;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Commands;

public class KkmCommandsManager : ICommandManager
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

        var hi = CommandsHistory.First(
            x => x.Command.Deserialize<DeviceCommand>()?.CommandId == deviceCommand.CommandId);

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
                OpenSession(kkm, hi);
                break;
            case Enums.CommandTypes.CashInOut: 
                CashInOut(kkm, hi);
                break;
            case Enums.CommandTypes.DoReport:
                DoReport(kkm, hi);
                break;
            case Enums.CommandTypes.PrintFiscalReceipt:
                PrintFiscalReceipt(kkm, hi);
                break;
        }
    }

    private static void PrintFiscalReceipt(FiscalRegistrarFacade kkm , CommandQueue cq)
    {
        var c = cq.Command.Deserialize<KkmPrintFiscalReceiptCommand>();
        var a = () => kkm.PrintFiscalReceipt(c.ReceiptData);

        SetResult(a, cq);
    }

    private static void CashInOut(FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var c = cq.Command.Deserialize<KkmCashInOutCommand>();
        var a = () => kkm.CashInOut(c.Sum, c.Cashier);

        SetResult(a, cq);
    }

    /// <summary>
    /// Открытие смены
    /// </summary>
    /// <param name="kkm">ККМ</param>
    /// <param name="cq">Объект очереди</param>
    private static void OpenSession(FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var c = cq.Command.Deserialize<KkmOpenSessionCommand>();
        var a = () => kkm.OpenSession(c.Cashier);

        SetResult(a, cq);
    }

    /// <summary>
    /// Выполнение отчета на ККМ
    /// </summary>
    /// <param name="kkm">ККМ</param>
    /// <param name="cq">Объект очереди</param>
    private static void DoReport(FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var c = cq.Command.Deserialize<KkmGetReportCommand>();
        var a = () => kkm.GetReport(c.ReportType, c.Cashier);

        SetResult(a, cq);
    }

    /// <summary>
    /// Запрос статуса ККМ
    /// </summary>
    /// <param name="kkm">ККМ</param>
    /// <param name="cq">Объект очереди</param>
    private static void GetStatus(FiscalRegistrarFacade kkm, CommandQueue cq)
    {
        var f = kkm.GetStatus;
        SetResult(f, cq);
    }
}