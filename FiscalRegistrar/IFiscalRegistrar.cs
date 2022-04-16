using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace devicesConnector;

/// <summary>
/// Фискальный регистратор, ККМ
/// </summary>
public interface IFiscalRegistrar : IDevice
{
    /// <summary>
    /// Открыть смену
    /// </summary>
    public void OpenSession(Cashier cashier);

    /// <summary>
    /// Снять отчет
    /// </summary>
    public void GetReport(Cashier cashier);

    /// <summary>
    /// Напечатать фискальный чек
    /// </summary>
    public void PrintFiscalReceipt();

    /// <summary>
    /// Напечатать НЕфискальный чек
    /// </summary>
    public void PrintNonFiscalReceipt();

    /// <summary>
    /// Внести наличные
    /// </summary>
    public void InsertCash(decimal sum, Cashier cashier);

    /// <summary>
    /// Изъять наличные
    /// </summary>
    public void TakeoutCash(decimal sum, Cashier cashier );

    //получить статус

    

}

/// <summary>
/// Кассир
/// </summary>
public class Cashier
{
    /// <summary>
    /// Имя
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    /// <summary>
    /// Налоговый идентификатор (ИНН)
    /// </summary>
    [JsonPropertyName("TaxId")]
    public string? TaxId { get; set; }
}

/// <summary>
/// Запись о товара в чеке
/// </summary>
public class ReceiptGood
{
    /// <summary>
    /// Название товара
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Штрих-код
    /// </summary>
    public string? Barcode { get; set; }
    /// <summary>
    /// Цена товара
    /// </summary>
    public decimal Price { get; set; }
    /// <summary>
    /// Скидка на товар (процент)
    /// </summary>
    public decimal Discount { get; set; }
    /// <summary>
    /// Кол-во товара
    /// </summary>
    public decimal Quantity { get; set; }

  public RuKkmInfo? RuKkmData { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }
}



public class FiscalCheck
{
    
}