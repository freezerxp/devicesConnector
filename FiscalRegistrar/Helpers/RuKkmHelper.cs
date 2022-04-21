using System.Text.Json;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;

namespace devicesConnector.FiscalRegistrar.Helpers;

public class RuKkmHelper
{
    public static string PrepareMarkCodeForFfd120(string code)
    {
        var FNC1 = Convert.ToChar(29);

        return code
            .Trim()
            .Replace(' ', FNC1);

    }

    ///  <summary>
    ///  Получение планируемый статус КМ (тег 2003)
    ///  </summary>
    ///  <param name="item">Товарная позиция</param>
    ///  <param name="type">Тип документа (продажа/возврат)</param>
    ///  <returns>Планируемый статус КМ (2003)</returns>
    public static Enums.EstimatedStatus GetMarkingCodeStatus(ReceiptItem item, Enums.ReceiptOperationTypes type)
    {

        var ffdData = item.CountrySpecificData
            .Deserialize<ReceiptItemData>()?.FfdData;

        if (ffdData == null)
        {
            throw new NullReferenceException();
        }

        if (ffdData.Unit != Enums.FfdUnitsIndex.Pieces)
        {
            return type == Enums.ReceiptOperationTypes.Sale ? Enums.EstimatedStatus.DryForSale : Enums.EstimatedStatus.DryReturn;
        }
        return type == Enums.ReceiptOperationTypes.Sale ? Enums.EstimatedStatus.PieceSold : Enums.EstimatedStatus.PieceReturn;
    }
}