using System;
using System.Diagnostics;

namespace ThinkPadScrollHelper
{
  public static class RichScrollDaemon
  {
    public static void RestartIfCrashed()
    {
      const string scrollBackgroundPath = @"C:\Program Files (x86)\Lenovo\ThinkPad Compact Keyboard with TrackPoint driver\HScrollFun.exe";
      const string scrollBackgroundName = "HScrollFun";

      var processesScroll = Process.GetProcessesByName(scrollBackgroundName);
      if (processesScroll.Length < 1)
      {
        Console.WriteLine($"---- Restart {scrollBackgroundName} ----");
        Process.Start(scrollBackgroundPath);
      }
    }
  }
}