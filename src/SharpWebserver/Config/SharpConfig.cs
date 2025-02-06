using System.Collections.Generic;

namespace SharpWebserver.Config;

public class SharpConfig
{
  public List<string> Prefixes { get; set; } = [
    "http://*",
  ];
  public bool SafeMode { get; set; } = false;
  public int PortNumber { get; set; } = 80;
}
