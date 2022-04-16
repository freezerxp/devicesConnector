using System.Text.Json.Serialization;

namespace devicesConnector;


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
    public Answer(Statuses status)
    {
        Status = status;
    }


    /// <summary>
    /// Статус операции
    /// </summary>
    public Statuses Status { get; set; }

    /// <summary>
    /// Сообщение
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Результат выполнения команды
    /// </summary>
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
        /// Операция выполнена, но с предупреждением
        /// </summary>
        Warning,

        /// <summary>
        /// Ошибка при выполнении операции
        /// </summary>
        Error
    }
}