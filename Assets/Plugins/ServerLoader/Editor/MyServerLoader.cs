using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
/// <summary>
/// 编辑器辅助测试脚本：webserver 自动加载器
/// 播放时，如果 webserver 没有运行则自动运行服务器
/// 停止播放时，不会处理已经开启的 webserver
/// 关闭此编辑器时，将会同步关闭开启的 webserver
/// </summary>
[InitializeOnLoad]
public static class MyServerLoader
{
    private static string path;
    private static Process process;

    static MyServerLoader()
    {
        path = Path.Combine(Application.streamingAssetsPath, "WebServer/MyWebServer.exe");
        if (File.Exists(path))
        {
            GetProcess();
            //无需 -= 程序集每次 Reload 都会清空事件列表
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
        }
        else
        {
            Debug.LogWarning("MyWebServer.exe 未找到。\n {path}");
        }
    }
    [MenuItem("Tools/关闭 WebServer", priority = 99)]
    private static void OnEditorQuitting()
    {
        process?.Kill();
        process = null;
        RefreshNotificationArea();
    }
    private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
    {
        if (obj == PlayModeStateChange.ExitingEditMode) // 当进入播放模式前，先运行 mywebserver.exe
        {
            RunWebServer();
        }
    }
    [MenuItem("Tools/开启 WebServer", priority = 98)]
    private static void RunWebServer()
    {
        if (GetProcess()) return; //1.先通过配置文件设置为启动就开服务
        CheckAutoRunServerConfig();
        process = Process.Start(path); //2. 启动应用
        var processHandle = IntPtr.Zero;
        EnumChildWindows(IntPtr.Zero, (h, l) =>
        {
            int length = GetWindowTextLength(h);
            StringBuilder windowName = new StringBuilder(length + 1);
            GetWindowText(h, windowName, windowName.Capacity);
            if (windowName.ToString().Contains("MyWebServer"))
            {
                GetWindowThreadProcessId(h, out int pid);
                if (pid == process.Id)
                {
                    processHandle = h;
                }
            }
            return processHandle == IntPtr.Zero;
        }, 0);
        if (processHandle == IntPtr.Zero)
        {
            Debug.Log($"{nameof(MyServerLoader)}: 未发现 mywebserver 窗口句柄");
        }
        ShowWindow(processHandle, 6); //3. 隐藏窗口
    }

    /// <summary>
    /// 在配置文件中申明启动程序后立马运行服务
    /// </summary>
    private static void CheckAutoRunServerConfig()
    {
        //[option] autostart=0
        // server.ini 中删除 vpath ，Release 后不能再映射到 工程目录中咯
        var path = Path.Combine(Application.streamingAssetsPath, "WebServer", "server.ini");
        var configArr = new List<string>(File.ReadAllLines(path));
        var index = configArr.IndexOf("autostart=0");
        if (index != -1)
        {
            configArr.Insert(index - 1, "autostart=1");
            configArr.RemoveAt(index + 1);
            File.WriteAllLines(path, configArr);
        }
    }
    [MenuItem("Tools/开启 WebServer", priority = 98, validate = true)]
    static bool Validate0() => !GetProcess();
    [MenuItem("Tools/关闭 WebServer", priority = 99, validate = true)]
    static bool Validate1() => GetProcess();
    private static bool GetProcess()
    {
        process = Process.GetProcessesByName("MyWebServer")
            .FirstOrDefault(v => v.MainModule?.FileName == path.Replace('/', '\\'));
        return null != process && null != process.MainModule;
    }
    #region 刷新 TaskBar Tray Icon
    public static void RefreshNotificationArea()
    {
        var notificationAreaHandle = GetNotificationAreaHandle();
        if (notificationAreaHandle == IntPtr.Zero)
        {
            Debug.Log("未找到 TaskBar-TrayIcon");
        }
        else
        {
            RefreshWindow(notificationAreaHandle);
        }
    }
    private static void RefreshWindow(IntPtr windowHandle)
    {
        const uint wmMousemove = 0x0200;
        RECT rect;
        GetClientRect(windowHandle, out rect);
        for (var x = 0; x < rect.right; x += 5)
            for (var y = 0; y < rect.bottom; y += 5)
                SendMessage(
                    windowHandle,
                    wmMousemove,
                    0,
                    (y << 16) + x);
    }
    private static IntPtr GetNotificationAreaHandle()
    {
        string title = System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN" ? "用户提示通知区域" : "User Promoted Notification Area";
        var systemTrayContainerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", string.Empty);
        var systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", string.Empty);
        var sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", string.Empty);
        var notificationAreaHandle = IntPtr.Zero;
        EnumChildWindows(sysPagerHandle, (h, l) =>
        {
            if (IsWindowVisible(h))
            {
                int length = GetWindowTextLength(h);
                StringBuilder windowName = new StringBuilder(length + 1);
                GetWindowText(h, windowName, windowName.Capacity);
                if (windowName.ToString() == title)
                {
                    notificationAreaHandle = h;
                }
            }
            return notificationAreaHandle == IntPtr.Zero;
        }, 0);
        return notificationAreaHandle;
    }
    #endregion

    #region user32.dll Wrapper
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
    [DllImport("user32.dll")]
    public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);
    public delegate bool CallBack(IntPtr hwnd, int lParam);
    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr handle, out RECT rect);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr handle, UInt32 message, Int32 wParam, Int32 lParam);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    struct RECT
    {
        public int left, top, right, bottom;
    }
    #endregion
}