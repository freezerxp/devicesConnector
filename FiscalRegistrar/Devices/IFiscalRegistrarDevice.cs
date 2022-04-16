namespace devicesConnector.Drivers;

public interface IFiscalRegistrarDevice
{
    public void PrintNonFiscalString(string str);

    public KkmStatus GetStatus();
}