using System.Text.Json.Serialization;
using devicesConnector.FiscalRegistrar.Objects;

namespace devicesConnector.Common;


/// <summary>
/// Абстрактный класс команды устройству
/// </summary>
public abstract  class DeviceCommand 
{
    /// <summary>
    /// Id устройства
    /// </summary>
    public int DeviceId { get; set; }

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