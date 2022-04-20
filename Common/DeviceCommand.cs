using System.Text.Json.Serialization;
using devicesConnector.FiscalRegistrar.Objects;

namespace devicesConnector.Common;


/// <summary>
/// Абстрактный класс команды устройству
/// </summary>
public   class DeviceCommand
{
    /// <summary>
    /// Идентификатор команды
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Id устройства
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Тип выполняемой команды
    /// </summary>
    public int CommandType { get; set; }

    /// <summary>
    /// Выполнение команды без постановки в очередь (для тестирования)
    /// </summary>
    public bool WithOutQueue { get; set; }

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
    /// Идентификатор команды
    /// </summary>
    public string CommandId { get; set; }

    /// <summary>
    /// Идентификатор устройства, на которое была отправлена команда
    /// </summary>
    public string? DeviceId { get; set; }

    

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
        /// Завершена с ошибкой
        /// </summary>
        Error, 

        /// <summary>
        /// Ожидание очереди
        /// </summary>
        Wait, 

        /// <summary>
        /// Выполняется
        /// </summary>
        Run
    }

  
}

/// <summary>
/// Объект ошибки для ответа
/// </summary>
public class ErrorObject
{


    public ErrorObject(Exception e)
    {
        Message = e.Message;
        StackTrace = e.StackTrace;

        if (e is KkmException kkmE)
        {
            DeviceErrorCode = kkmE.KkmErrorCode;
            DeviceErrorDescription = kkmE.KkmErrorDescription;
            ErrorCode = (int)kkmE.Error;
        }
    }
    /// <summary>
    /// Сообщение
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    /// Стэк
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Код ошибки (внутренний)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Код ошибки, возвращаемый устройством (драйвером)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DeviceErrorCode { get; set; }

    /// <summary>
    /// Сообщение об ошибке, возвращаемое устройством (драйвером)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceErrorDescription { get; set; }
}