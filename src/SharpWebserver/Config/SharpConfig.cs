using System.Text.Json.Serialization;

namespace SharpWebserver.Config;

public class SharpConfig
{
  [JsonInclude]
  public bool SafeMode;
  [JsonInclude]
  public int PortNumber = 80;

}
