using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MahApps.Metro;
using Spedit.Interop;
using Spedit.UI;
using Spedit.Properties;

namespace Spedit
{
    public static class Program
    {
        public const string ProgramInternalVersion = "13";

        public static MainWindow MainWindow;
        public static OptionsControl OptionsObject;
        public static ConfigList ConfigList;

        public static bool RCCKMade;

        [STAThread]
        public static void Main(string[] args) 
        {
            bool mutexReserved;
            using (Mutex appMutex = new Mutex(true, "SpeditGlobalMutex", out mutexReserved))
            {
                if (mutexReserved)
				{
					bool ProgramIsNew = false;
#if !DEBUG
                    try
                    {
#endif
                        Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if !DEBUG
						ProfileOptimization.SetProfileRoot(Environment.CurrentDirectory);
						ProfileOptimization.StartProfile("Startup.Profile");
#endif
						//todo implement own updater
						OptionsObject = OptionsControl.Load(out ProgramIsNew);


                        ConfigList = ConfigLoader.Load();
                        foreach (Config config in ConfigList.Configs.Where(config => config.Name == OptionsObject.Program_SelectedConfig))
                        {
                            ConfigList.CurrentConfig = ConfigList.Configs.IndexOf(config);
                            break;
                        }
                        if (!OptionsObject.Program_UseHardwareAcceleration)
                        {
                            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
						}
#if !DEBUG
						if (ProgramIsNew)
						{
							if (Translations.AvailableLanguageIDs.Length > 0)
							{
								splashScreen.Close(new TimeSpan(0, 0, 1));
								var languageWindow = new UI.Interop.LanguageChooserWindow(Translations.AvailableLanguageIDs, Translations.AvailableLanguages);
								languageWindow.ShowDialog();
								string potentialSelectedLanguageID = languageWindow.SelectedID;
								if (!string.IsNullOrWhiteSpace(potentialSelectedLanguageID))
								{
									OptionsObject.Language = potentialSelectedLanguageID;
									Translations.LoadLanguage(potentialSelectedLanguageID);
								}
								splashScreen.Show(false, true);
							}
						}
#endif
						MainWindow = new MainWindow();
                        new PipeInteropServer(MainWindow).Start();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        File.WriteAllText("CRASH_" + Environment.TickCount.ToString() + ".txt", BuildExceptionString(e, "SPEDIT LOADING"));
                        MessageBox.Show("An error occured while loading." + Environment.NewLine + "A crash report was written in the editor-directory.",
                            "Error while Loading",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Environment.Exit(Environment.ExitCode);
                    }
#endif
                    Application app = new Application();
#if !DEBUG
                    try
                    {
                        if (OptionsObject.Program_CheckForUpdates)
                        {
                            UpdateCheck.Check(true);
                        }
#endif
                        app.Startup += App_Startup;
                        app.Run(MainWindow);
                        OptionsControl.Save();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        File.WriteAllText("CRASH_" + Environment.TickCount.ToString() + ".txt", BuildExceptionString(e, "SPEDIT MAIN"));
                        MessageBox.Show("An error occured." + Environment.NewLine + "A crash report was written in the editor-directory.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Environment.Exit(Environment.ExitCode);
                    }
#endif
                }
                else
                {
                    try
                    {
                        StringBuilder sBuilder = new StringBuilder();
                        bool addedFiles = false;
                        for (int i = 0; i < args.Length; ++i)
                        {
                            if (!string.IsNullOrWhiteSpace(args[i]))
                            {
                                FileInfo fInfo = new FileInfo(args[i]);
                                if (fInfo.Exists)
                                {
                                    string ext = fInfo.Extension.ToLowerInvariant().Trim('.', ' ');
                                    //todo fix this?
                                    if (ext == "sp" || ext == "inc" || ext == "txt" || ext == "smx")
                                    {
                                        addedFiles = true;
                                        sBuilder.Append(fInfo.FullName);
                                        if ((i + 1) != args.Length)
                                        { sBuilder.Append("|"); }
                                    }
                                }
                            }
                        }
                        if (addedFiles)
                        { PipeInteropClient.ConnectToMasterPipeAndSendData(sBuilder.ToString()); }
                    }
                    catch (Exception) { } //dont fuck the user up with irrelevant data
                }
            }
        }

		public static void MakeRCCKAlert()
		{
			if (RCCKMade)
			{
				return;
			}
			RCCKMade = true;
			MessageBox.Show("All FTP/RCon passwords are now encrypted wrong!" + Environment.NewLine + "You have to replace them!",
				"Created new crypto key", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void ClearUpdateFiles()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.exe", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                FileInfo fInfo = new FileInfo(file);
                if (fInfo.Name.StartsWith("updater_", StringComparison.CurrentCultureIgnoreCase))
                {
                    fInfo.Delete();
                }
            }
        }

		private static void App_Startup(object sender, StartupEventArgs e)
		{
			
			Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
			ThemeManager.ChangeAppStyle(Application.Current,
									ThemeManager.GetAccent("Green"),
									ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1
		}

		private static string BuildExceptionString(Exception e, string sectionName)
        {
            StringBuilder outString = new StringBuilder();
            outString.AppendLine("Section: " + sectionName);
            outString.AppendLine(".NET Version: " + Environment.Version);
            outString.AppendLine("OS: " + Environment.OSVersion.VersionString);
            outString.AppendLine("64 bit OS: " + ((Environment.Is64BitOperatingSystem) ? "TRUE" : "FALSE"));
            outString.AppendLine("64 bit mode: " + ((Environment.Is64BitProcess) ? "TRUE" : "FALSE"));
            outString.AppendLine("Dir: " + Environment.CurrentDirectory);
            outString.AppendLine("Working Set: " + (Environment.WorkingSet / 1024) + " kb");
            outString.AppendLine("Installed UI Culture: " + CultureInfo.InstalledUICulture);
            outString.AppendLine("Current UI Culture: " + CultureInfo.CurrentUICulture);
            outString.AppendLine("Current Culture: " + CultureInfo.CurrentCulture);
            outString.AppendLine();
            int eNumber = 1;
            for (; ; )
            {
                if (e == null)
                {
                    break;
                }
                outString.AppendLine("Exception " + eNumber);
                outString.AppendLine("Message:");
                outString.AppendLine(e.Message);
                outString.AppendLine("Stacktrace:");
                outString.AppendLine(e.StackTrace);
                outString.AppendLine("Source:");
                outString.AppendLine(e.Source ?? "null");
                outString.AppendLine("HResult Code:");
                outString.AppendLine(e.HResult.ToString());
                outString.AppendLine("Helplink:");
                outString.AppendLine(e.HelpLink ?? "null");
                if (e.TargetSite != null)
                {
                    outString.AppendLine("Targetsite Name:");
                    outString.AppendLine(e.TargetSite.Name);
                }
                e = e.InnerException;
                eNumber++;
            }
            return (eNumber - 1) + Environment.NewLine + outString;
        }
    }
}
