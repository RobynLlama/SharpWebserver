/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using System.Collections.Generic;

namespace SharpWebserver;

public partial class ListenServer
{
  public static void Usage()
  {
    int paddingRight = 20;
    string paddingLeft = new(' ', 2);

    void WriteSwitchInfo(string switches, string description)
    {
      Console.Write(paddingLeft + switches.PadRight(paddingRight));
      Console.WriteLine("| " + description);
    }

    Console.WriteLine();
    Console.WriteLine("Options: ");

    WriteSwitchInfo("-h, --help", "Show this screen");
    WriteSwitchInfo("-p, --portable", "Launch in portable mode using the current directory instead of appdata");
    WriteSwitchInfo("-l, --license", "output this program's license information and exit");
  }
  public static void ProcessArguments(in string[] args)
  {

    //The reason we use a queue is to let switches consume the next item
    //and pop it off the queue so the next iteration doesn't process an
    //argument instead of a switch
    var items = new Queue<string>(args);

    while (items.Count > 0)
    {

      var item = items.Dequeue();

      void StopProcess()
      {
        ExitNow = true;
        items.Clear();
      }

      switch (item)
      {
        case "--portable":
        case "-p":
          BaseDir = AppContext.BaseDirectory;
          break;
        case "--help":
        case "-h":
          Usage();
          StopProcess();
          break;
        case "--license":
        case "-l":
          Console.Write(LICENSE);
          StopProcess();
          break;
        default:
          Logger.LogWarning($"Unknown switch {item}, try --help for options");
          StopProcess();
          break;
      }
    }
  }
}
