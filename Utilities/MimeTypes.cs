using System.Collections.Generic;

namespace SharpWebserver;

public static partial class Utilities
{

  private static readonly Dictionary<string, string> MimeTypes = new()
  {
    //TEXT or HTML
    {"htm", "text/html; charset=UTF-8"},
    {"html", "text/html; charset=UTF-8"},
    {"css", "text/html; charset=UTF-8"},
    {"cs", "text/html; charset=UTF-8"},
    {"txt", "text/plain; charset=UTF-8"},

    //SCRIPT
    {"js", "text/javascript"},

    //IMAGES
    {"ico", "image/x-icon"},
    {"png", "image/png"},
    {"jpg", "image/jpeg"},
    {"jpeg", "image/jpeg"},
    {"gif", "image/gif"},
  };

  public static string Default = "application/octet-stream";

  public static string GetMimeType(string fileType)
  {
    if (MimeTypes.TryGetValue(fileType.ToLowerInvariant(), out var mime))
      return mime;

    return Default;
  }
}
