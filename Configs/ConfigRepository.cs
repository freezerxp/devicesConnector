using System.Text.Json;

namespace devicesConnector.Configs;

/// <summary>
/// Репозиторий настроек
/// </summary>
public class ConfigRepository
{

        private string _configFilePath = "./Examples/Configs/config.json";


    /// <summary>
    /// Получить настройки
    /// </summary>
    /// <returns></returns>
    public Config Get()
    {

        var json = System.IO.File.ReadAllText(_configFilePath);

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
        var json = JsonSerializer.Serialize(config);

       File.WriteAllText(_configFilePath, json);
    }
}