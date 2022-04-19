using System.Text.Json.Nodes;
using devicesConnector.FiscalRegistrar.Objects;

namespace devicesConnector;

/// <summary>
/// Запись о товара в чеке
/// </summary>
public class ReceiptItem
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

    /// <summary>
    /// Данные, специфичные для региона
    /// </summary>
    public JsonNode? CountrySpecificData { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Индекс налоговой ставки (НДС)
    /// </summary>
    public int? TaxRateIndex { get; set; }

    /// <summary>
    /// Номер секции (отдела)
    /// </summary>
    public int DepartmentIndex { get; set; } = 1;

    /// <summary>
    /// Сумма скидки на позицию
    /// </summary>
    public decimal DiscountSum =>
        Math.Round(Discount / 100M * Price * Quantity, 2, MidpointRounding.AwayFromZero);

}