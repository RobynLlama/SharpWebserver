using System.IO;
using Tomlyn;

namespace SharpWebserver;

public static class ConfigManager
{
  public static readonly TomlModelOptions Options = new()
  {
    ConvertPropertyName = x => x,
  };

  public static T? LoadConfig<T>(string fileName) where T : class, new()
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

        try
        {
          var thing = Toml.ToModel<T>(objectData, options: Options);
          if (thing is not null)
          {
            Logger.LogTrace("Finished loading config from disk");
            return thing;
          }
        }
        catch
        {
          Logger.LogError("Unable to parse data as generic type T", [
            ("Size", objectData.Length),
            ("Type", typeof(T))
          ]);
        }
      }
    }

    return null;
  }

  public static void SaveConfig(string fileName, object configObject)
  {
    var path = Path.Combine(ListenServer.ConfigDir, fileName);
    var configFile = new FileInfo(path);

    using (var writer = new StreamWriter(new FileStream(configFile.FullName, FileMode.Truncate)))
    {
      writer.Write(Toml.FromModel(configObject, options: Options));
      Logger.LogInfo($"Wrote out {fileName}");
    }
  }
}
