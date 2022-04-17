using System.Text.Json.Serialization;

namespace devicesConnector.Common;


/// <summary>
/// Абстрактный класс команды устройству
/// </summary>
public abstract  class DeviceCommand 
{
    /// <summary>
    /// Параметры подключения к устройству
    /// </summary>
    public DeviceConnection Connection { get; set; }

}


/// <summary>
/// Ответ на команду
/// </summary>
public class Answer
{
    public Answer(Statuses status, object? data)
    {
        Status = status;
        Data = data;
    }


    /// <summary>
    /// Статус операции
    /// </summary>
    public Statuses Status { get; set; }

    /// <summary>
    /// Результат выполнения команды
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }


    /// <summary>
    /// Статусы ответа
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Statuses
    {
        /// <summary>
        /// Операция успешно выполнена
        /// </summary>
        Ok,

        /// <summary>
        /// Ошибка при выполнении операции
        /// </summary>
        Error
    }
}