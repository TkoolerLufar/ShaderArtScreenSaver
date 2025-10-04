using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UnityScreenSaverLauncher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Show config window from context menu
            if (args.Length == 0)
            {
                return ShowConfig(nint.Zero);
            }

            var firstArg = args[0].ToUpper();

            // Show config window from screen saver settings
            if (firstArg.StartsWith("/C"))
            {
                var hWnd = args.Length > 1 ? nint.Parse(firstArg.StartsWith("/C:") ? firstArg[3..] : args[1]) : nint.Zero;
                return ShowConfig(hWnd);
            }

            // Show preview
            if (firstArg.StartsWith("/P"))
            {
                var hWndStr = firstArg.StartsWith("/P:") ? firstArg[3..] : args[1];
                var hWnd = nint.Parse(hWndStr);

                SetPreviewModeToPlayerPrefs(true);

                return RunAsChildProcess($"-parentHWND {hWndStr}", hWnd);
            }

            // Start screen saver
            if (firstArg == "/S")
            {
                SetPreviewModeToPlayerPrefs(false);
                var unityArgs = "-screen-fullscreen 1 -window-mode borderless";
                if (Screen.PrimaryScreen is Screen primaryScreen)
                {
                    unityArgs = $"{unityArgs} -screen-width {primaryScreen.Bounds.Width} -screen-height {primaryScreen.Bounds.Height}";
                }
                return RunAsChildProcess(unityArgs, nint.Zero);
            }

            return ShowConfig(nint.Zero);
        }

        /// <summary>
        /// Show a config UI.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        static int ShowConfig(nint hWnd)
        {
            MessageBox.Show(NativeWindow.FromHandle(hWnd),
                "This screen saver has no options that you can set.", "Unity Screen Saver");
            return 0;
        }

        /// <summary>
        /// Set whether this is preview or not.
        /// </summary>
        /// <param name="isPreview">true when this is preview</param>
        static void SetPreviewModeToPlayerPrefs(bool isPreview)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
            string companyName = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company;

            // Change the registry key by your Unity setting
            string regKey = @$"Software\{companyName}\{productName}";
            using var playerPrefs =
                Registry.CurrentUser.OpenSubKey(regKey, true) ??
                Registry.CurrentUser.CreateSubKey(regKey, true);
            playerPrefs.SetValue("isPreview", isPreview ? 1 : 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Run Unity Player as a child process.
        /// </summary>
        /// <param name="hInstance">Instance of the parent process.</param>
        /// <param name="hPrevInstance">Unused since Win32.</param>
        /// <param name="UnityArguments">Command line arguments.</param>
        /// <param name="nShowCmd">Prefered window mode.</param>
        /// <returns>The exit code.</returns>
        static int RunAsChildProcess(string UnityArguments, nint ParentWindowHandle)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product;

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = $"{productName}.exe",
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = UnityArguments,
                CreateNoWindow = ParentWindowHandle != nint.Zero,
                ErrorDialog = ParentWindowHandle != nint.Zero,
                ErrorDialogParentHandle = ParentWindowHandle,
            });
            if (process is null)
            {
                return 1;
            }
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
