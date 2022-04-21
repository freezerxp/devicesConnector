using System.Text.Json.Nodes;
using devicesConnector.FiscalRegistrar.Objects;

namespace devicesConnector;

/// <summary>
/// Кассовый чек
/// </summary>
public class ReceiptData
{



    /// <summary>
    /// Фискальный тип чека
    /// </summary>
    public Enums.ReceiptFiscalTypes FiscalType { get; set; }

    /// <summary>
    /// Тип операции: продажа, возврат и т.п.
    /// </summary>
    public Enums.ReceiptOperationTypes OperationType { get; set; }

    /// <summary>
    /// Кассир
    /// </summary>
    public Cashier Cashier { get; set; }

    /// <summary>
    /// Покупатель
    /// </summary>
    public Contractor? Contractor { get; set; }


    /// <summary>
    /// Перечень позиций в чеке
    /// </summary>
    public List<ReceiptItem> Items { get; set; }

    /// <summary>
    /// Перечень платежей по чеку
    /// </summary>
    public List<ReceiptPayment> Payments { get; set; }

    /// <summary>
    /// Специфичные для страны данные (ФФД, Маркировка)
    /// </summary>
    public JsonNode CountrySpecificData { get; set; }

    /// <summary>
    /// Печатать ли бумажный чек 
    /// </summary>
    public bool IsPrintReceipt { get; set; } = true;

}

