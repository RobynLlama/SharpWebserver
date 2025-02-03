using System.IO;
using System.Text.Json;

namespace SharpWebserver;

public static class ConfigManager
{
  public static T? LoadConfig<T>(string fileName) where T : class
  {
    var path = Path.Combine(ListenServer.ConfigDir, fileName);
    var config = new FileInfo(path);

    if (config.Exists)
    {
      Logger.LogInfo("Reading a config from disk", [
        ("config-path", path)
      ]);

      using (var reader = config.OpenText())
      {
        string objectData = reader.ReadToEnd();
        var thing = JsonSerializer.Deserialize<T>(objectData);

        if (thing is not null)
        {
          Logger.LogTrace("Finished loading config from disk");
          return thing;
        }

        Logger.LogError("Unable to parse data as generic type T", [
          ("Size", objectData.Length),
          ("Type", typeof(T))
        ]);
      }
    }

    return null;
  }

  public static void SaveConfig(string fileName, object configObject)
  {
    var path = Path.Combine(ListenServer.ConfigDir, fileName);
    var configFile = new FileInfo(path);

    using (var writer = configFile.OpenWrite())
    {
      JsonSerializer.Serialize(writer, configObject);
      Logger.LogInfo($"Wrote out {fileName}");
    }
  }
}
