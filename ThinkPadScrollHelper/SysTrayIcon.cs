using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThinkPadScrollHelper
{
  public static class SysTrayIcon
  {
    private static NotifyIcon _notifyIcon;
    private static ContextMenu _contextMenu;
    private static IContainer _components;
    private static MenuItem _menuItem;

    public static void Init()
    {
      _components = new Container();
      _menuItem = new MenuItem()
      {
        Index = 0,
        Text = "E&xit",
      };

      _menuItem.Click += _menuItem_Click;

      _contextMenu = new ContextMenu();
      _contextMenu.MenuItems.Add(_menuItem);

      _notifyIcon = new NotifyIcon(_components)
      {
        ContextMenu = _contextMenu,
        Text = "Thinkpad Scroll Helper",
        Icon = Properties.Resources.app,
        Visible = true,
      };

      _notifyIcon.Click += _notifyIcon_Click;

      _notifyIcon.Visible = true;
      Application.Run();
    }

    private static void _notifyIcon_Click(object sender, EventArgs e)
    {
      Console.WriteLine("Hi");
    }

    private static void _menuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }
  }
}