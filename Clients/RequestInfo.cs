using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SharpWebserver.Clients;

public class RequestInfo
{

  public readonly Uri? Request;
  public readonly bool HasPost = false;
  public byte[] PostData = [];
  public readonly Dictionary<string, string> Arguments = [];
  public string? UserAgent;
  public string? ContentType;
  public RequestInfo(NetworkStream read)
  {
    // Read incoming data from the client
    string document = "/";
    Uri? host = null;

    //Logger.LogTrace("Reading what the client sent");

    MemoryStream backingStream = new();
    byte[] buffer = new byte[1024];
    uint postLength = 0u;
    int retries = 1;
    int bytesRead = 0;

    while (true)
    {
      while (read.DataAvailable)
      {
        bytesRead += read.Read(buffer, 0, buffer.Length);
        backingStream.Write(buffer);
      }

      {
        if (retries > 0)
        {
          //Logger.LogTrace("Pausing for client");
          Thread.Sleep(25);
          retries--;
          continue;
        }
        break;
      }
    }

    //Logger.LogTrace("Finished reading client info", [("Size", bytesRead)]);
    backingStream.Seek(0, SeekOrigin.Begin);
    StreamReader reader = new(backingStream, Encoding.UTF8);
    string? line;

    while ((line = reader.ReadLine()) is not null && !string.IsNullOrEmpty(line))
    {
      //Logger.LogTrace("Reading a line", [("Line", line)]);
      //catch the post or get part
      if (line.StartsWith("get", StringComparison.InvariantCultureIgnoreCase))
      {
        //Logger.LogTrace("Reading GET");
        document = line.Split(' ')[1];
      }
      else if (line.StartsWith("post", StringComparison.InvariantCultureIgnoreCase))
      {
        //Logger.LogTrace("Reading POST");
        document = line.Split(' ')[1];
        HasPost = true;
      }
      else
      {
        var subs = line.Split(':');
        if (subs.Length > 1)
        {
          var key = subs[0].ToLowerInvariant().Trim();
          var val = subs[1].Trim();

          //Logger.LogTrace("Assigning item", [
          //("Key", key),
          //("Value", val)
          //]);

          switch (key)
          {
            case "host":
              try
              {
                host = new("https://" + val);
              }
              catch (Exception ex)
              {
                Logger.LogError("Error while parsing host", [("Line", line), ("Exception", ex)]);
              }
              break;
            case "content-length":
              if (uint.TryParse(val, out var result))
                postLength = result;
              else
                Logger.LogWarning("Failed to parse content-length", [("Value", val)]);
              break;
            case "user-agent":
              UserAgent = val;
              break;
            case "content-type":
              ContentType = val;
              break;
          }
        }
      }
    }

    if (host is not null)
      Request = new(host, document);

    /*Logger.LogInfo("Request processed", [
        ("Host", host),
        ("Document", document),
        ("Request", Request),
        ("Post", HasPost)
    ]);*/

    string PostString = string.Empty;
    PostData = new byte[postLength];

    if (HasPost && postLength > 0)
    {
      //Logger.LogInfo("Fetching post string");
      backingStream.Seek(bytesRead - postLength, SeekOrigin.Begin);
      //Logger.LogTrace("Reading backing stream into post data", [("Read", backingStream.Read(PostData)), ("Size", postLength)]);
      PostString = Encoding.UTF8.GetString(PostData);
    }

    //Logger.LogInfo("Parsing arguments", [(
    //("PostArgs", PostString)
    //)]);

    foreach (byte value in PostData)
    {
      Console.Write($"{value:X2} ");
    }
    Console.WriteLine();

    if (Request is not null)
      Utilities.ParseArguments(Request, Arguments);
    if (HasPost)
      Utilities.ParseArguments(PostString, Arguments);
  }

}
