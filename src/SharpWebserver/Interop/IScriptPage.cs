/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Net;

namespace SharpWebserver.Interop;
public interface IScriptPage
{
  public byte[] CreatePage(HttpListenerRequest request);
}
