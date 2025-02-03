using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpWebserver.Config;

public class ClientBlockList
{
  [JsonInclude]
  public List<string> BlockList = [];
}
