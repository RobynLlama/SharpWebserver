using System.Collections.Generic;
using SharpWebserver.Clients;

namespace SharpWebserver.Interop;
public interface IScriptPage
{
  public byte[] CreatePage(ConnectedClient client, Dictionary<string, string> arguments);
}
