using System.Text.Json.Serialization;

namespace devicesConnector;

public class RuKkmInfo
{
    public RuFfdInfo FfdData { get; set; }
}

/// <summary>
/// 
/// </summary>
public class RuFfdInfo
{
    public Enums.FfdCalculationSubjects Subject { get; set; }

    public Enums.FfdCalculationMethods Method { get; set; }
}