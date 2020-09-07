using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThinkPadScrollHelper
{
  public static class Util
  {
    private static IntPtr m_found;

    public static IntPtr FindMousePropertiesWindow()
    {
      m_found = IntPtr.Zero;
      Win32Api.EnumWindows(new Win32Api.WNDENUMPROC(EnumWindowCallBack), IntPtr.Zero);
      return m_found;
    }

    private static string m_findOption;

    public static IntPtr FindChildWindowByCaption(IntPtr hwnd, string caption)
    {
      m_findOption = "@Text:" + caption.ToLowerInvariant();
      m_found = IntPtr.Zero;
      Win32Api.EnumChildWindows(hwnd, EnumChildWindowCallBack, IntPtr.Zero);
      return m_found;
    }

    public static void HideWindow(IntPtr hwnd)
    {
      Win32Api.ShowWindow(hwnd, 0);
    }

    public static void CloseWindow(IntPtr hwnd)
    {
      Win32Api.CloseWindow(hwnd);
    }

    public static IntPtr FindChildWindowByClassName(IntPtr hwnd, string className)
    {
      m_findOption = "@Class:" + className;
      m_found = IntPtr.Zero;
      Win32Api.EnumChildWindows(hwnd, EnumChildWindowCallBack, IntPtr.Zero);
      return m_found;
    }

    private static bool EnumChildWindowCallBack(IntPtr hWnd, IntPtr lparam)
    {
      int textLen = Win32Api.GetWindowTextLength(hWnd);
      if (textLen >= 1)
      {
        StringBuilder winText = new StringBuilder(textLen + 1);
        Win32Api.GetWindowText(hWnd, winText, winText.Capacity);
        // Console.WriteLine(tsb.ToString());
        if ("@Text:" + winText.ToString().ToLowerInvariant() == m_findOption)
        {
          m_found = hWnd;
          return false;
        }
      }

      //ウィンドウのクラス名を取得する
      StringBuilder winClass = new StringBuilder(256);
      Win32Api.GetClassName(hWnd, winClass, winClass.Capacity);
      if ("@Class:" + winClass.ToString() == m_findOption)
      {
        m_found = hWnd;
        return false;
      }

      return true;
    }

    private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
    {
      //ウィンドウのタイトルの長さを取得する
      int textLen = Win32Api.GetWindowTextLength(hWnd);
      if (0 < textLen)
      {
        //ウィンドウのタイトルを取得する
        StringBuilder winText = new StringBuilder(textLen + 1);
        Win32Api.GetWindowText(hWnd, winText, winText.Capacity);

        //ウィンドウのクラス名を取得する
        StringBuilder winClass = new StringBuilder(256);
        Win32Api.GetClassName(hWnd, winClass, winClass.Capacity);

        //結果を表示する
        if (winText.ToString().Contains("Mouse Properties"))
        {
          Console.WriteLine("WindowClass:" + winClass.ToString());
          Console.WriteLine("WindowText:" + winText.ToString());
          m_found = hWnd;
          return false;
        }
      }

      //すべてのウィンドウを列挙する
      return true;
    }
  }

  public static class MouseWatcher
  {
    public static string GetProcessPathUnderMouseCursor()
    {
      // 現在のマウス位置.
      var p = Cursor.Position;
      // Console.WriteLine($"{p.X}, {p.Y}");

      // マウス配下のウィンドウ.
      var hwnd = Win32Api.WindowFromPoint(p);
      if (hwnd == default) return "";
      // Console.WriteLine("0x" + hwnd.ToString("X8"));

      // ウィンドウのプロセス判断
      Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
      if (pid == 0) return "";

      // プロセス名
      Process process = Process.GetProcessById((int)pid);
      if (process == null) return "";
      try
      {
        return process.MainModule.FileName.ToLowerInvariant();
      }
      catch
      {
        return "";
      }
    }
  }

  internal static class Program
  {
    private static void Main()
    {
      try
      {
        Task trayTask = Task.Run(SysTrayIcon.Init);
        Task mainTask = Task.Run(MainLogic);
        Task.WaitAny(trayTask, mainTask);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: " + ex.Message);
      }
    }

    private static void MainLogic()
    {
      // 必要なプロパティダイアログの表示.
      RichScrollDialog.Init();

      string lastProcessPath = "";
      bool? lastRichScroll = null;
      while (true)
      {
        // 定期スリープ.
        Thread.Sleep(80);

        // バックグラウンドプロセスの監視. 死んでたら再起動.
        RichScrollDaemon.RestartIfCrashed();

        // マウス配下のプロセス判断
        string processPath = MouseWatcher.GetProcessPathUnderMouseCursor();
        // if (processPath == "") continue;

        // 特定プロセスではリッチスクロール機能をOFFにする
        if (lastProcessPath != processPath)
        {
          lastProcessPath = processPath;
          Console.WriteLine($"Process: {processPath}");

          bool richScroll = true;
          if (processPath.Contains(@"\microsoft visual studio\")) richScroll = false;
          if (processPath.EndsWith(@"\syswow64\cmd.exe")) richScroll = false;
          else if (processPath.EndsWith(@"\scriptedsandbox64.exe")) richScroll = false;
          else if (processPath.EndsWith(@"\ssms.exe")) richScroll = false;
          else if (processPath.EndsWith(@"\sourcetree.exe")) richScroll = false;
          else if (processPath.EndsWith(@"\hscrollfun.exe")) richScroll = false;
          else if (processPath.EndsWith(@"\linqpad.exe")) richScroll = false;
          else if (processPath?.Length == 0) richScroll = true;

          // ウィンドウに応じてモードを切り替える
          if (richScroll != lastRichScroll)
          {
            lastRichScroll = richScroll;
            Console.WriteLine($"RichScroll: {richScroll}");
            RichScrollDialog.SetEnabled(lastRichScroll.Value);
          }
        }
      }
    }
  }
}