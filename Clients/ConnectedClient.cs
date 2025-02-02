using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using SharpWebserver.Caching;

namespace SharpWebserver.Clients;

public class ConnectedClient
{
  public uint ClientID { get; private set; }
  public TcpClient Client { get; private set; }
  public EndPoint Remote { get; private set; }
  public bool Stale { get; protected set; }
  private static uint NextClientID = 0u;
  protected static uint GetNextClientID => ++NextClientID;

  public ConnectedClient(TcpClient client, EndPoint remote)
  {
    ClientID = GetNextClientID;
    Client = client;
    Remote = remote;

    Logger.LogInfo($"New Client", [
      ("ID", ClientID),
      ("Remote", remote)
    ]);
  }

  public void BecomeStale(uint code, string reason, string feedback)
  {
    Stale = true;
    Client.GetStream().Write(Utilities.GenerateResponse(code, reason, $"<h1>Connection Refused</h1><p>{feedback}</p>", "text/html", true));
    Client.Client.Disconnect(false);
  }

  public bool CheckIn()
  {
    if (Stale)
      return false;

    if (!Client.Connected)
      return false;

    //Logger.LogTrace("Checking if client sent us anything");

    if (Client.Available > 0)
    {

      var info = new RequestInfo(Client.GetStream());

      if (info.Request is null)
      {
        Logger.LogError("Bad request, missing host or document");
        BecomeStale(400u, "Malformed Request", "Request is malformed or damaged");
        return false;
      }

      var (code, reason, contents, contentType) = RequestDocumentFromWebRoot(info.Request, info.Arguments);

      if (Stale)
        return false;

      byte[] responseData = Utilities.GenerateResponse(code, reason, contents, contentType);

      SendResponse(responseData);

      Logger.LogInfo($"Responding to a request", [
        ("Request", info.Request),
        ("Content-Type", contentType),
        ("Status", code),
        ("Reason", reason),
        ("Sent", responseData.Length + " bytes"),
      ]);
    }

    return true;
  }

  public void SendResponse(byte[] response)
  {
    Client.GetStream().Write(response, 0, response.Length);
    Client.GetStream().Flush();
  }
  public (uint code, string reason, byte[] contents, string contentType) RequestDocumentFromWebRoot(Uri request, Dictionary<string, string> arguments)
  {
    var localPath = request.AbsolutePath;
    string resolvedPath = Path.GetFullPath(Path.Combine(ListenServer.WebRoot, "." + localPath));

    if (!resolvedPath.StartsWith(ListenServer.WebRoot))
    {
      Logger.LogWarning("Attempted to access below root", [
        ("Client", ClientID),
        ("Request", localPath),
        ("Resolved", resolvedPath),
        ("Remote", Client.Client.RemoteEndPoint)
      ]);

      Utilities.BanRemote(Remote);
      BecomeStale(403u, "Forbidden", "Access to this resource is forbidden. Your client information has been logged");
      return (0u, "", [], "");
    }

    string fileName = Path.GetFileName(resolvedPath);
    if (string.IsNullOrEmpty(fileName))
    {
      resolvedPath = Path.Combine(resolvedPath, "Index.cs");
    }

    Logger.LogInfo("Finished path resolution", [
      ("Original", localPath),
      ("Resolved", resolvedPath)
    ]);

    FileInfo file = new(resolvedPath);

    if (!file.Exists)
    {
      return (404u, "Not Found", Encoding.UTF8.GetBytes("<h2>404: File Not Found</h2><p>Resource not found</p>"), "text/html");
    }

    var name = Path.GetFileName(resolvedPath).ToLowerInvariant();
    var chunks = name.Split('.');
    string contentType;

    if (chunks.Length < 2)
      contentType = "text/plain";
    else
      contentType = Utilities.GetMimeType(chunks[1]);

    if (name.EndsWith(".cs"))
    {
      var script = PageCache.FetchPage(file);

      if (script is null)
        return (500u, "Server Error", Encoding.UTF8.GetBytes("The requested resource failed to build"), "text/html");

      try
      {
        var data = script.CreatePage(this, arguments);
        return (200u, "OK", data, contentType);
      }
      catch (Exception ex)
      {
        return (500u, "Server Error", Encoding.UTF8.GetBytes($"The requested resource encountered an exception while running {ex.ToString().Replace("<", "&gt;")}"), "text/html");
      }

    }
    else
    {
      using var reader = file.OpenRead();
      byte[] fileContents = new byte[file.Length];
      reader.Read(fileContents, 0, (int)file.Length);
      return (200u, "OK", fileContents, contentType);
    }
  }
}
