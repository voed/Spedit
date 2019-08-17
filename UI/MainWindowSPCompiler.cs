﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Spedit.Interop;
using Spedit.UI.Components;
using Spedit.Utils;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        private List<string> compiledFiles = new List<string>();
        private List<string> nonUploadedFiles = new List<string>();
        private List<string> compiledFileNames = new List<string>();

        private bool InCompiling;
        private void Compile_SPScripts(bool All = true)
        {
            if (InCompiling) { return; }
            Command_SaveAll();
            InCompiling = true;
            compiledFiles.Clear();
            compiledFileNames.Clear();
            nonUploadedFiles.Clear();
            Config c = Program.ConfigList.Current;
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
                if (filesToCompile.Count > 0)
                {
                    ErrorResultGrid.Items.Clear();
                    StringBuilder stringOutput = new StringBuilder();
                    Regex errorFilterRegex = new Regex(@"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    foreach (var file in filesToCompile)
                    {
						if (!InCompiling) //pressed escape
						{
							PressedEscape = true;
							break;
						}
                        FileInfo fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);
                        StatusLine_StatusText.Text = $"{Properties.Resources.Compiling} \"{fileInfo.Name}...\"";
                        ProcessUITasks();
                        if (fileInfo.Exists)
                        {
                            using (Process process = new Process())
                            {
                                process.StartInfo.WorkingDirectory = fileInfo.DirectoryName;
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;


                                string destinationFileName = Path.GetFileNameWithoutExtension(file) + ".amxx";

                                string outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile))
                                    File.Delete(outFile);

                                string errorFile = Environment.CurrentDirectory + $@"\sourcepawn\errorfiles\error_{Environment.TickCount}{file.GetHashCode():X}.txt";
                                if (File.Exists(errorFile))
                                    File.Delete(errorFile);

                                StringBuilder includeDirectories = new StringBuilder();
                                foreach (string dir in c.SMDirectories)
                                {
                                    includeDirectories.Append(" -i\"" + dir + "\\include\"");
                                }

                                var includeStr = includeDirectories.ToString();

                                process.StartInfo.Arguments = $"\"{fileInfo.FullName}\" -o\"{outFile}\" -e\"{errorFile}\"{includeStr}";// // todo add debug level
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
                                    StatusLine_StatusText.Text = "";
                                }
                                if (!InCompiling) //cannot await in catch
                                {
                                    MessageBox.Show(Properties.Resources.SPCompNotStarted, Properties.Resources.Error);
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
                                        stringOutput.AppendLine(Properties.Resources.CompileErroFileError);
                                    }
                                }
                                stringOutput.AppendLine(Properties.Resources.Done);
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
                                ProcessUITasks();
                            }
                        }
                    }
					if (!PressedEscape)
					{
						//CompileOutput.Text = stringOutput.ToString();
						if (c.AutoCopy)
						{
							Copy_Plugins(true);
						}
						if (CompileOutputRow.Height.Value < 11.0)
						{
							CompileOutputRow.Height = new GridLength(200.0);
						}
					}
                    StatusLine_StatusText.Text = "";
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.SPCompNotFound);
            }
            InCompiling = false;
        }

        public void Copy_Plugins(bool OvertakeOutString = false)
        {
            if (compiledFiles.Count > 0)
            {
                nonUploadedFiles.Clear();
                int copyCount = 0;
                Config c = Program.ConfigList.Current;
                if (!string.IsNullOrWhiteSpace(c.CopyDirectory))
                {
                    StringBuilder stringOutput = new StringBuilder();
                    foreach (string t in compiledFiles)
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
                                stringOutput.AppendLine($"{Properties.Resources.Copied}: " + t);
                                ++copyCount;
                                if (c.DeleteAfterCopy)
                                {
                                    File.Delete(t);
                                    stringOutput.AppendLine($"{Properties.Resources.Deleted}: " + t);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            stringOutput.AppendLine($"{Properties.Resources.FailCopy}: " + t);
                        }
                    }
                    if (copyCount == 0)
                    {
                        stringOutput.AppendLine(Properties.Resources.NoFilesCopy);
                    }
                    if (OvertakeOutString)
                    {
                        //CompileOutput.AppendText(stringOutput.ToString());
                    }
                    else
                    {
                        //CompileOutput.Text = stringOutput.ToString();
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

            Config c = Program.ConfigList.Current;
            if ((string.IsNullOrWhiteSpace(c.FTPHost)) || (string.IsNullOrWhiteSpace(c.FTPUser)))
            {
                return;
            }
            StringBuilder stringOutput = new StringBuilder();
            try
            {
                FTP ftp = new FTP(c.FTPHost, c.FTPUser, c.FTPPassword);
                foreach (string file in nonUploadedFiles)
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
                            stringOutput.AppendLine($"{Properties.Resources.Uploaded}: " + file);
                        }
                        catch (Exception e)
                        {
                            stringOutput.AppendLine(string.Format(Properties.Resources.ErrorUploadFile, file, uploadDir));
                            stringOutput.AppendLine($"{Properties.Resources.Details}: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                stringOutput.AppendLine(Properties.Resources.ErrorUpload);
                stringOutput.AppendLine($"{Properties.Resources.Details}: " + e.Message);
            }
            stringOutput.AppendLine(Properties.Resources.Done);
            //CompileOutput.Text = stringOutput.ToString();
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

            Config c = Program.ConfigList.Current;
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
            Dispatcher?.Invoke(UpdateWindowTitle);
            ServerProcess.WaitForExit();
            ServerProcess.Dispose();
            ServerIsRunning = false;
            Dispatcher?.Invoke(() =>
            {
                if (IsLoaded)
                {
                    UpdateWindowTitle();
                }
            });
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
