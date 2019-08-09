using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.Utils;

namespace Spedit.UI
{
    public partial class MainWindow : MetroWindow
    {
        private List<string> compiledFiles = new List<string>();
        private List<string> nonUploadedFiles = new List<string>();
        private List<string> compiledFileNames = new List<string>();

        private bool InCompiling;
        private async void Compile_SPScripts(bool All = true)
        {
            if (InCompiling) { return; }
            Command_SaveAll();
            InCompiling = true;
            compiledFiles.Clear();
            compiledFileNames.Clear();
            nonUploadedFiles.Clear();
            var c = Program.Configs[Program.SelectedConfig];
            FileInfo spCompInfo = null;
            bool SpCompFound = false;
			bool PressedEscape = false;
            foreach (string dir in c.SMDirectories)
            {
                spCompInfo = new FileInfo(Path.Combine(dir, "amxxpc.exe"));
                if (spCompInfo.Exists)
                {
                    SpCompFound = true;
                    break;
                }
            }
            if (SpCompFound)
            {
                List<string> filesToCompile = new List<string>();
                if (All)
                {
                    EditorElement[] editors = GetAllEditorElements();
                    if (editors == null)
					{
						InCompiling = false;
						return;
                    }

                    filesToCompile.AddRange(from editor in editors where editor.CompileBox.IsChecked != null && editor.CompileBox.IsChecked.Value select editor.FullFilePath);
                }
                else
                {
                    EditorElement ee = GetCurrentEditorElement();
                    if (ee == null)
					{
						InCompiling = false;
						return;
                    }
                    /*
                    ** I've struggled a bit here. Should i check, if the CompileBox is checked 
                    ** and only compile if it's checked or should it be ignored and compiled anyway?
                    ** I decided, to compile anyway but give me feedback/opinions.
                    */
                    if (ee.FullFilePath.EndsWith(".sma"))
                    {
                        filesToCompile.Add(ee.FullFilePath);
                    }
                }
                int compileCount = filesToCompile.Count;
                if (compileCount > 0)
                {
                    ErrorResultGrid.Items.Clear();
                    var progressTask = await this.ShowProgressAsync(Program.Translations.Compiling, "", false, MetroDialogOptions);
                    progressTask.SetProgress(0.0);
                    StringBuilder stringOutput = new StringBuilder();
                    Regex errorFilterRegex = new Regex(@"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    for (int i = 0; i < compileCount; ++i)
                    {
						if (!InCompiling) //pressed escape
						{
							PressedEscape = true;
							break;
						}
                        string file = filesToCompile[i];
                        progressTask.SetMessage(file);
                        ProcessUITasks();
                        var fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);
                        if (fileInfo.Exists)
                        {
                            using (Process process = new Process())
                            {
                                process.StartInfo.WorkingDirectory = fileInfo.DirectoryName;
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;
                                var destinationFileName = ShortenScriptFileName(fileInfo.Name) + "amxx";
                                var outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile)) { File.Delete(outFile); }
                                string errorFile = Environment.CurrentDirectory + @"\sourcepawn\errorfiles\error_" + Environment.TickCount + "_" + file.GetHashCode().ToString("X") + "_" + i + ".txt";
                                if (File.Exists(errorFile)) { File.Delete(errorFile); }

                                StringBuilder includeDirectories = new StringBuilder();
                                foreach (string dir in c.SMDirectories)
                                {
                                    includeDirectories.Append(" -i\"" + dir + "\\include\"");
                                }

                                string includeStr = string.Empty;
                                includeStr = includeDirectories.ToString();

                                process.StartInfo.Arguments =
                                    "\"" + fileInfo.FullName + "\" -o\"" + outFile + "\" -e\"" + errorFile + "\"" + includeStr;// + " -O" + c.OptimizeLevel.ToString() + " -v" + c.VerboseLevel.ToString();
                                progressTask.SetProgress((i + 1 - 0.5d) / compileCount);
                                string execResult = ExecuteCommandLine(c.PreCmd, fileInfo.DirectoryName, c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                                if (!string.IsNullOrWhiteSpace(execResult))
                                {
                                    stringOutput.AppendLine(execResult.Trim('\n', '\r'));
                                }
                                ProcessUITasks();
                                try
                                {
                                    process.Start();
                                    process.WaitForExit();
                                }
                                catch (Exception)
                                {
                                    InCompiling = false;
                                }
                                if (!InCompiling) //cannot await in catch
                                {
                                    await progressTask.CloseAsync();
                                    await this.ShowMessageAsync(Program.Translations.SPCompNotStarted, Program.Translations.Error, MessageDialogStyle.Affirmative, MetroDialogOptions);
									return;
                                }
                                if (File.Exists(errorFile))
                                {
                                    string errorStr = File.ReadAllText(errorFile);
                                    stringOutput.AppendLine(errorStr.Trim('\n', '\r'));
                                    MatchCollection mc = errorFilterRegex.Matches(errorStr);
                                    for (int j = 0; j < mc.Count; ++j)
                                    {
                                        ErrorResultGrid.Items.Add(new ErrorDataGridRow
                                        {
                                            file = mc[j].Groups["file"].Value.Trim(),
                                            line = mc[j].Groups["line"].Value.Trim(),
                                            type = mc[j].Groups["type"].Value.Trim(),
                                            details = mc[j].Groups["details"].Value.Trim()
                                        });
                                    }
                                    try
                                    {
                                        File.Delete(errorFile);
                                    }
                                    catch (Exception)
                                    {
                                        stringOutput.AppendLine(Program.Translations.CompileErroFileError);
                                    }
                                }
                                stringOutput.AppendLine(Program.Translations.Done);
                                if (File.Exists(outFile))
                                {
                                    compiledFiles.Add(outFile);
                                    nonUploadedFiles.Add(outFile);
                                    compiledFileNames.Add(destinationFileName);
                                }
                                string execResult_Post = ExecuteCommandLine(c.PostCmd, fileInfo.DirectoryName, c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                                if (!string.IsNullOrWhiteSpace(execResult_Post))
                                {
                                    stringOutput.AppendLine(execResult_Post.Trim('\n', '\r'));
                                }
                                stringOutput.AppendLine();
                                progressTask.SetProgress((i + 1) / ((double)compileCount));
                                ProcessUITasks();
                            }
                        }
                    }
					if (!PressedEscape)
					{
						progressTask.SetProgress(1.0);
						CompileOutput.Text = stringOutput.ToString();
						if (c.AutoCopy)
						{
							Copy_Plugins(true);
						}
						if (CompileOutputRow.Height.Value < 11.0)
						{
							CompileOutputRow.Height = new GridLength(200.0);
						}
					}
                    await progressTask.CloseAsync();
                }
            }
            else
            {
                await this.ShowMessageAsync(Program.Translations.Error, Program.Translations.SPCompNotFound, MessageDialogStyle.Affirmative, MetroDialogOptions);
            }
            InCompiling = false;
        }

        public void Copy_Plugins(bool OvertakeOutString = false)
        {
            if (compiledFiles.Count > 0)
            {
                nonUploadedFiles.Clear();
                int copyCount = 0;
                var c = Program.Configs[Program.SelectedConfig];
                if (!string.IsNullOrWhiteSpace(c.CopyDirectory))
                {
                    StringBuilder stringOutput = new StringBuilder();
                    foreach (var t in compiledFiles)
                    {
                        try
                        {
                            FileInfo destFile = new FileInfo(t);
                            if (destFile.Exists)
                            {
                                string destinationFileName = destFile.Name;
                                string copyFileDestination = Path.Combine(c.CopyDirectory, destinationFileName);
                                File.Copy(t, copyFileDestination, true);
                                nonUploadedFiles.Add(copyFileDestination);
                                stringOutput.AppendLine($"{Program.Translations.Copied}: " + t);
                                ++copyCount;
                                if (c.DeleteAfterCopy)
                                {
                                    File.Delete(t);
                                    stringOutput.AppendLine($"{Program.Translations.Deleted}: " + t);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            stringOutput.AppendLine($"{Program.Translations.FailCopy}: " + t);
                        }
                    }
                    if (copyCount == 0)
                    {
                        stringOutput.AppendLine(Program.Translations.NoFilesCopy);
                    }
                    if (OvertakeOutString)
                    {
                        CompileOutput.AppendText(stringOutput.ToString());
                    }
                    else
                    {
                        CompileOutput.Text = stringOutput.ToString();
                    }
                    if (CompileOutputRow.Height.Value < 11.0)
                    {
                        CompileOutputRow.Height = new GridLength(200.0);
                    }
                }
            }
        }

        public void FTPUpload_Plugins()
        {
            if (nonUploadedFiles.Count <= 0)
            {
                return;
            }
            var c = Program.Configs[Program.SelectedConfig];
            if ((string.IsNullOrWhiteSpace(c.FTPHost)) || (string.IsNullOrWhiteSpace(c.FTPUser)))
            {
                return;
            }
            StringBuilder stringOutput = new StringBuilder();
            try
            {
                FTP ftp = new FTP(c.FTPHost, c.FTPUser, c.FTPPassword);
                foreach (var file in nonUploadedFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        string uploadDir;
                        if (string.IsNullOrWhiteSpace(c.FTPDir))
                        {
                            uploadDir = fileInfo.Name;
                        }
                        else
                        {
                            uploadDir = c.FTPDir.TrimEnd('/') + "/" + fileInfo.Name;
                        }
                        try
                        {
                            ftp.upload(uploadDir, file);
                            stringOutput.AppendLine($"{Program.Translations.Uploaded}: " + file);
                        }
                        catch (Exception e)
                        {
                            stringOutput.AppendLine(string.Format(Program.Translations.ErrorUploadFile, file, uploadDir));
                            stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                stringOutput.AppendLine(Program.Translations.ErrorUpload);
                stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
            }
            stringOutput.AppendLine(Program.Translations.Done);
            CompileOutput.Text = stringOutput.ToString();
            if (CompileOutputRow.Height.Value < 11.0)
            {
                CompileOutputRow.Height = new GridLength(200.0);
            }
        }

        public bool ServerIsRunning;
        public Process ServerProcess;
        public Thread ServerCheckThread;

        public void Server_Start()
        {
            if (ServerIsRunning)
            { return; }
            var c = Program.Configs[Program.SelectedConfig];
            string serverOptionsPath = c.ServerFile;
            if (string.IsNullOrWhiteSpace(serverOptionsPath))
            {
                return;
            }
            FileInfo serverExec = new FileInfo(serverOptionsPath);
            if (!serverExec.Exists)
            {
                return;
            }
            try
            {
                if (serverExec.DirectoryName != null)
                    ServerProcess = new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = true,
                            FileName = serverExec.FullName,
                            WorkingDirectory = serverExec.DirectoryName,
                            Arguments = c.ServerArgs
                        }
                    };
                ServerCheckThread = new Thread(ProcessCheckWorker);
                ServerCheckThread.Start();
            }
            catch (Exception)
            {
                ServerProcess?.Dispose();
            }

        }

        private void ProcessCheckWorker()
        {
            try
            {
                ServerProcess.Start();
            }
            catch (Exception)
            {
                return;
            }
            ServerIsRunning = true;
            Program.MainWindow.Dispatcher?.Invoke(() =>
            {
                EnableServerAnim.Begin();
                UpdateWindowTitle();
            });
            ServerProcess.WaitForExit();
            ServerProcess.Dispose();
            ServerIsRunning = false;
            Program.MainWindow.Dispatcher?.Invoke(() =>
            {
                if (Program.MainWindow.IsLoaded)
                {
                    DisableServerAnim.Begin();
                    UpdateWindowTitle();
                }
            });
        }


        private string ShortenScriptFileName(string fileName)
        {
            if (fileName.EndsWith(".sma", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - 3);
            }
            return fileName;
        }

        private string ExecuteCommandLine(string code, string directory, string copyDir, string scriptFile, string scriptName, string pluginFile, string pluginName)
        {
            code = ReplaceCMDVaraibles(code, directory, copyDir, scriptFile, scriptName, pluginFile, pluginName);
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            string batchFile = (new FileInfo(Path.Combine("sourcepawn\\temp\\", Environment.TickCount + "_" + ((uint)code.GetHashCode() ^ (uint)directory.GetHashCode()) + "_temp.bat"))).FullName;
            File.WriteAllText(batchFile, code);
            string result = null;
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = directory;
                process.StartInfo.Arguments = "/c \"" + batchFile + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                }
            }
            File.Delete(batchFile);
            return result;
        }

        private string ReplaceCMDVaraibles(string CMD, string scriptDir, string copyDir, string scriptFile, string scriptName, string pluginFile, string pluginName)
        {
            CMD = CMD.Replace("{editordir}", Environment.CurrentDirectory.Trim('\\'));
            CMD = CMD.Replace("{scriptdir}", scriptDir);
            CMD = CMD.Replace("{copydir}", copyDir);
            CMD = CMD.Replace("{scriptfile}", scriptFile);
            CMD = CMD.Replace("{scriptname}", scriptName);
            CMD = CMD.Replace("{pluginfile}", pluginFile);
            CMD = CMD.Replace("{pluginname}", pluginName);
            return CMD;
        }
    }

    public class ErrorDataGridRow
    {
        public string file { set; get; }
        public string line { set; get; }
        public string type { set; get; }
        public string details { set; get; }
    }
}
