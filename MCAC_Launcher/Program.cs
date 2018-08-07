using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MCAC_Launcher
{
    class Program
    {
        public static Localization localization;
        private static LaunchGUI gui;
        private static Thread GuiThread;

        static Program()
        {
            // Initialize localization
            switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "de":
                    localization = new GermanLocalization();
                    break;
                case "ru":
                    localization = new RussianLocalization();
                    break;
                default:
                    localization = new DefaultLocalization();
                    break;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show(localization.InvalidUsage(), "4lpha Anti Cheat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // run GUI in dedicated thread
            gui = new LaunchGUI();
            GuiThread = new Thread(() =>
            {
                gui.Show();
                Application.Run(gui);
            });
            GuiThread.Start();

            gui.SetStatus("Checking for 4AC Updates");
#if !DEBUG
            try
            {
                if (!HttpApi.IsThisUpToDate())
                {
                    MessageBox.Show(localization.OutOfDate(), "4lpha Anti Cheat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Process.Start("https://mcac.alphaantileak.net/");
                    Application.Exit();
                }
            }
            catch (Exception e)
            {
                // May happen if this version gets too old
                MessageBox.Show("Update Check failed. Maybe there is a new version? https://mcac.alphaantileak.net/ \n\n\n" + e.ToString(), "4lpha Anti Cheat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Console.WriteLine(e);
            }
#endif

            string session = null;

            gui.SetStatus("Logging in (4AC)");
            try
            {
                session = Registry.CurrentUser.OpenSubKey(@"Software\4lphaAntiCheat").GetValue("AAL_Session") as string;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            if (session == null)
            {
                session = HttpApi.CreateAccount();

                Registry.CurrentUser.CreateSubKey(@"Software\4lphaAntiCheat").SetValue("AAL_Session", session);
            }
            else if (!HttpApi.IsValidSession(session))
            {
                /* This can have a few reasons:
                 * - Something went wrong on our end
                 * - Something changed the registry value
                 * - You got banned by our system
                 */
                if (MessageBox.Show("Login failed. If this message appears again today you should contact support. Continue?", "4lpha Anti Cheat", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    session = HttpApi.CreateAccount();

                    Registry.CurrentUser.CreateSubKey(@"Software\4lphaAntiCheat").SetValue("AAL_Session", session);
                }
                else Application.Exit();
            }

            gui.SetStatus("Checking for AAL Launcher Update");
            if (!HttpApi.IsLauncherUpToDate())
            {
                gui.SetStatus("Downloading AAL Launcher Update");
                HttpApi.DownloadLauncher();
            }

            gui.SetStatus("Authorizing");
            HttpApi.AddApp(session);

            gui.SetStatus("Launching AAL");

            // Build AAL command line
            var commandLineBuilder = new StringBuilder();
            commandLineBuilder.Append("f1b31908-fbc7-4b65-b781-3cbb2f2f189d ").Append(session);

            foreach (var arg in args)
            {
                if (arg.Contains(' '))
                {
                    commandLineBuilder.Append(" \"").Append(arg.Replace("\"", "\\\"")).Append('"');
                }
                else
                {
                    commandLineBuilder.Append(' ').Append(arg);
                }

                if (arg.ToLower().StartsWith("-xmx")) // set heap
                {
                    int size_multiplier = arg.ToLower().EndsWith("g") ? 1024 : 1;
                    uint mb = uint.Parse(arg.Substring(4, arg.Length - 5));

                    var si = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        FileName = Directory.GetCurrentDirectory() + "//" + HttpApi.Launcher,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        Arguments = "settings set \"{\\\"heap\\\":" + (mb * size_multiplier) + "}\""
                    };
                    Process.Start(si).WaitForExit();
                }
            }

            // launch AAL
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = Directory.GetCurrentDirectory() + "//" + HttpApi.Launcher,
                Arguments = commandLineBuilder.ToString(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
            var proc = new Process
            {
                StartInfo = startInfo
            };
            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            gui.SafeClose();
        }

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Error.WriteLine(e.Data);
        }

        private static void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // process output
            if (e.Data == null) return;
            Console.WriteLine(e.Data);
            if (e.Data == "AAL_STATUS_LAUNCHED")
            {
                gui.SafeHide();
            }
            else if (e.Data == "AAL_LAUNCHER_STATUS_CHECKING_FOR_UPDATES")
            {
                gui.SetStatus("Checking for Updates (AAL)");
            }
            else if (e.Data.StartsWith("AAL_LAUNCHER_STATUS_DOWNLOAD"))
            {
                gui.SetStatus("Downloading AAL Update");
            }
            else if (e.Data.StartsWith("AAL_LAUNCHER_STATUS_ERROR_INCOMPATIBLE_SOFTWARE_"))
            {
                MessageBox.Show("Incompatible software detected. Please uninstall the following software: " + e.Data.Substring("AAL_LAUNCHER_STATUS_ERROR_INCOMPATIBLE_SOFTWARE_".Length), "4lpha Anti Cheat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Data == "AAL_STATUS_CONNECTED")
            {
                gui.SetStatus("Connected to AAL");
            }
            else if (e.Data == "AAL_STATUS_LOGIN_SUCCESS")
            {
                gui.SetStatus("Logged in (AAL)");
            }
            else if (e.Data.StartsWith("AAL_STATUS_DOWNLOAD_SIZE_"))
            {
                gui.SetStatus("Downloading");
            }
        }
    }
}
