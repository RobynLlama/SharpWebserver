/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Net;
using System.Net.Sockets;
using CSScriptLib;
using SharpWebserver.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpWebserver;
partial class ListenServer
{
  public static string BaseDir { get; private set; } = "";
  public static string WebRoot { get; private set; } = "";
  public static string ConfigDir { get; private set; } = "";
  public static string IncludesDir { get; private set; } = "";
  public static IEvaluator ScriptRunner = CSScript.Evaluator.ReferenceAssembly(typeof(ListenServer).Assembly);
  public static bool SafeMode { get; private set; } = true;
  public static string Version { get; private set; } = string.Empty;
  public static string LICENSE => """
  ---------------------------------------------------------------------
  SharpWebserver  Copyright (C) 2025  Robyn <Robyn@mamallama.dev>
    This program comes with ABSOLUTELY NO WARRANTY;
    This is free software, and you are welcome to redistribute it
    under certain conditions; See the included license for details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <https://www.gnu.org/licenses/>
  ---------------------------------------------------------------------
  
  """;

  static void Main(string[] args)
  {

    Console.WriteLine(LICENSE);

    //Setup connected clients
    List<ConnectedClient> clients = [];
    List<ConnectedClient> reapedClients = [];

    var ver = typeof(ListenServer).Assembly.GetName().Version;

    if (ver is null)
    {
      Logger.LogError("Somehow booted up without a version!");
      return;
    }

    BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RobynLlama", "SharpWebserver");
    WebRoot = Path.Combine(BaseDir, "www");
    ConfigDir = Path.Combine(BaseDir, "config");
    IncludesDir = Path.Combine(BaseDir, "includes");
    Version = $"v{ver.Major}.{ver.Minor}";

    Utilities.EnsureDirectory(BaseDir);
    Utilities.EnsureDirectory(WebRoot);
    Utilities.EnsureDirectory(ConfigDir);
    Utilities.EnsureDirectory(IncludesDir);

    Utilities.SecurityPolicy.LoadBlockList();

    Logger.LogInfo("Startup", [
      ("BaseDir", BaseDir),
      ("Version", Version),
      ("SafeMode", SafeMode),
    ]);

    // Define the local endpoint for the socket.
    // For this example, we'll use localhost and port 5000.
    IPAddress ipAddress = IPAddress.Any; // Use IPAddress.Parse("127.0.0.1") for localhost
    int port = 8080;

    // Create a TCP listener to accept client connections.
    TcpListener listener = new(ipAddress, port);

    //Register the input handler
    var InputHandler = Task.Run(HandleInputAsync);

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

        if (!Utilities.SecurityPolicy.AllowClient(remote))
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
        if (ExitNow)
        {
          Logger.LogInfo("Exit requested by user");
          break;
        }

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
    catch (Exception ex)
    {
      Logger.LogError("Exception!", [
        ("Type", ex.GetType().Name),
        ("Stack", ex.StackTrace)
      ]);
    }
    finally
    {
      // Stop listening for new clients.
      listener.Stop();
    }

    //Perform cleanup and eventually save configurations here, etc
    Utilities.SecurityPolicy.SaveBlockList();

    Logger.LogInfo("Goodbye!");
  }

}
