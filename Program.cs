using System.Net;
using System.Net.Sockets;
using CSScriptLib;
using SharpWebserver.Clients;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpWebserver;
class ListenServer
{
  public static string BaseDir { get; private set; } = "";
  public static string WebRoot { get; private set; } = "";
  public static string ConfigDir { get; private set; } = "";
  public static string IncludesDir { get; private set; } = "";
  public static IEvaluator ScriptRunner = CSScript.Evaluator.ReferenceAssembly(typeof(ListenServer).Assembly);
  public static bool SafeMode { get; private set; } = false;

  static void Main()
  {
    //Setup connected clients
    List<ConnectedClient> clients = [];
    List<ConnectedClient> reapedClients = [];

    BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RobynLlama", "SimpleServer");
    WebRoot = Path.Combine(BaseDir, "www");
    ConfigDir = Path.Combine(BaseDir, "config");
    IncludesDir = Path.Combine(BaseDir, "includes");

    Utilities.EnsureDirectory(BaseDir);
    Utilities.EnsureDirectory(WebRoot);
    Utilities.EnsureDirectory(ConfigDir);
    Utilities.EnsureDirectory(IncludesDir);

    Logger.LogInfo("Startup", [
      ("BaseDir", BaseDir),
      ("SafeMode", SafeMode),
    ]);

    // Define the local endpoint for the socket.
    // For this example, we'll use localhost and port 5000.
    IPAddress ipAddress = IPAddress.Any; // Use IPAddress.Parse("127.0.0.1") for localhost
    int port = 5000;

    // Create a TCP listener to accept client connections.
    TcpListener listener = new(ipAddress, port);

    try
    {
      // Start listening for client requests.
      listener.Start();
      Logger.LogInfo("Server starting up", [
        ("Bound IP", ipAddress),
        ("Bound Port", port),
        ("Status", "Waiting for connections")
      ]);

      static ConnectedClient? TryAcceptClient(TcpClient client)
      {
        if (client.Client.RemoteEndPoint is not EndPoint remote || remote.AddressFamily != AddressFamily.InterNetwork)
        {
          Logger.LogWarning("Refusing a client with bad or unsupported IP information");
          var response = Utilities.GenerateResponse(421u, "Misdirected Request", "<h1>Misdirected Request</h1><p>Unable to service your device's IP configuration</p>", "text/html", true);
          client.GetStream().Write(response, 0, response.Length);
          client.Close();
          return null;
        }

        if (!Utilities.AllowClient(remote))
        {
          Logger.LogWarning("Refusing a client due to security policy");
          var response = Utilities.GenerateResponse(403u, "Client Forbidden", "<h1>Client Forbidden</h1><p>Access denied, your client details have been logged</p>", "text/html");
          client.GetStream().Write(response, 0, response.Length);
          client.Close();
          return null;
        }

        return new(client, remote);
      }

      #region Main Loop

      while (true)
      {
        //Check for pending requests
        if (listener.Pending())
        {
          //Logger.LogTrace("Client connected");
          if (TryAcceptClient(listener.AcceptTcpClient()) is ConnectedClient accepted)
          {
            clients.Add(accepted);
          }
        }

        //Handle all requests from all clients
        foreach (var client in clients)
        {
          if (!client.CheckIn())
          {
            reapedClients.Add(client);
          }
        }

        foreach (var client in reapedClients)
        {
          Console.WriteLine($"Reaping a client: {client.ClientID}");
          clients.Remove(client);
        }

        reapedClients.Clear();
      }

      #endregion

    }
    catch (SocketException e)
    {
      Console.WriteLine($"SocketException: {e}");
    }
    finally
    {
      // Stop listening for new clients.
      listener.Stop();
    }

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();
  }

}
