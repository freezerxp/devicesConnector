using System.Text.Json;

namespace devicesConnector.Configs;

/// <summary>
/// Репозиторий настроек
/// </summary>
public class ConfigRepository
{
    /// <summary>
    /// Получить настройки
    /// </summary>
    /// <returns></returns>
    public Config Get()
    {
        var configFilePath = "./Examples/Configs/config.json";

        var json = System.IO.File.ReadAllText(configFilePath);

        var config = JsonSerializer.Deserialize<Config>(json);

        return config;
    }

    /// <summary>
    /// Сохранить настройки
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Save(Config config)
    {
        throw new NotImplementedException();
    }
}