namespace devicesConnector.Helpers;

public static class CommonHelper
{
    /// <summary>
    /// Инициализация COM/Active-x объекта
    /// </summary>
    /// <param name="programId"></param>
    /// <returns></returns>
    public static dynamic CreateObject(string programId)
    {

            var objType = Type.GetTypeFromProgID(programId, true);
            dynamic obj = Activator.CreateInstance(objType);
            
            return obj;
      
    }
}