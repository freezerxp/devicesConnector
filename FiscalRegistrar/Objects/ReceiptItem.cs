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

    public RuKkmInfo? RuKkmData { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }
}