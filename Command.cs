using System.Text.Json.Serialization;

namespace devicesConnector;

public class Command
{
    /// <summary>
    /// Тип устройства
    /// </summary>
   
    public DeviceTypes DeviceType { get; set; }

    /// <summary>
    /// Параметры подключения к устройству
    /// </summary>
  
    public DeviceConnection Connection { get; set; }

    /// <summary>
    /// Команда на устройство
    /// </summary>
    public CommandData Data { get; set; }
}



/// <summary>
/// Тип устройства
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceTypes
{
    /// <summary>
    /// ККМ
    /// </summary>
    FiscalRegistrar,

    /// <summary>
    /// Весы
    /// </summary>
    Scale,

    /// <summary>
    /// Весы с печатью этикеток
    /// </summary>
    ScaleWithPrinter,
}




public class CommandData 
{
    public string Action { get; set; }
    public object Data { get; set; }
}

public class KkmCommand 
{
    public KkmCommand(CommandData data)
    {
        if (Enum.TryParse(data.Action,true, out KkmActions action))
        {
            Action = action;

        }

    }

    public KkmActions Action { get; set; }


}

public enum KkmActions
{
    PrintNonFiscalReceipt,
    PrintFiscalReceipt
}


