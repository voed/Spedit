using Spedit.Interop;
using Spedit.UI.Components;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using Spedit.UI.Components.TextMarker;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        readonly Regex _errorFilterRegex =
            new Regex(
                @"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

        private bool InCompiling;

        //todo async this
        private void Compile_SPScript()
        {
            if (InCompiling)
            {
                return;
            }
            Command_Save();
            InCompiling = true;
            Config c = Program.ConfigList.Current;
            FileInfo spCompInfo = null;
            bool SpCompFound = false;

            foreach (string dir in c.SMDirectories)
            {
                //todo add setting
                spCompInfo = new FileInfo(Path.Combine(dir, "amxxpc.exe"));
                if (spCompInfo.Exists)
                {
                    SpCompFound = true;
                    break;
                }
            }

            if (!SpCompFound)
            {
                InCompiling = false;
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.SPCompNotFound);
                return;
            }

            EditorElement ee = GetCurrentEditorElement();
            if (ee == null || !ee.FullFilePath.EndsWith(".sma"))
            {
                InCompiling = false;
                return;
            }
            
            ErrorResultGrid.Items.Clear();
            CompileOutput.Text = "";

            StringBuilder stringOutput = new StringBuilder();
            FileInfo fileInfo = new FileInfo(ee.FullFilePath);
            stringOutput.AppendLine(fileInfo.Name);

            ProcessUITasks();
            
            if (fileInfo.Exists)
            {
                StatusLine_StatusText.Text = $"{Properties.Resources.Compiling} \"{fileInfo.Name} ...\"";
                CompOutTabControl.SelectedIndex = 0;
                using (Process process = new Process())
                {
                    string destinationFileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + ".amxx";

                    string outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                    if (File.Exists(outFile))
                        File.Delete(outFile);

                    string compOutput = "";

                    StringBuilder includeDirectories = new StringBuilder();
                    foreach (string dir in c.SMDirectories)
                    {
                        includeDirectories.Append(" -i\"" + Path.Combine(dir, "include") + "\"");
                    }

                    process.StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = fileInfo.DirectoryName,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = spCompInfo.FullName,
                        Arguments =
                            $"\"{fileInfo.FullName}\" -o\"{outFile}\" {includeDirectories}", // // todo add debug level
                        RedirectStandardOutput = true
                    };

#if DEBUG
                    //CompileOutput.Text += $"\"{fileInfo.FullName}\" -o\"{outFile}\" {includeDirectories}\n";
#endif

                    string execResult = ExecuteCommandLine(c.PreCmd, fileInfo.DirectoryName, c.CopyDirectory,
                        fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                    if (!string.IsNullOrWhiteSpace(execResult))
                    {
                        stringOutput.AppendLine(execResult.Trim('\n', '\r'));
                    }

                    ProcessUITasks();
                    try
                    {
                        process.Start();
                        compOutput = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(Properties.Resources.SPCompNotStarted, Properties.Resources.Error);
                        InCompiling = false;
                        StatusLine_StatusText.Text = "";
                    }

                    compOutput = ReformatCompOutput(compOutput);
                    stringOutput.Append(compOutput);

                    MatchCollection mc = _errorFilterRegex.Matches(compOutput);
                    foreach (Match match in mc)
                    {
                        string lineMatch = match.Groups["line"].Value.Trim();
                        string typeMatch = match.Groups["type"].Value.Trim();
                        ErrorResultGrid.Items.Add(new ErrorDataGridRow
                        {
                            file = match.Groups["file"].Value.Trim(),
                            line = lineMatch,
                            type = typeMatch,
                            details = match.Groups["details"].Value.Trim()
                        });
                        DocumentLine line = ee.editor.Document.Lines[int.Parse(lineMatch)-1];
                        ITextMarker marker = ee.textMarkerService.Create(line.Offset, line.Length);
                        marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                        marker.MarkerColor = typeMatch.Contains("error") ? Colors.Red : Colors.Green;

                    }
                    if (mc.Count > 0)
                    {
                        CompOutTabControl.SelectedIndex = 1; //todo get rid of this
                    }

                    if (process.ExitCode != 0)
                    {
                        StatusLine_StatusText.Text = "Error";
                    }
                    else
                    {
                        StatusLine_StatusText.Text = "Success";
                        string execResult_Post = ExecuteCommandLine(c.PostCmd, fileInfo.DirectoryName,
                            c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                        if (!string.IsNullOrWhiteSpace(execResult_Post))
                        {
                            stringOutput.AppendLine(execResult_Post.Trim('\n', '\r'));
                        }
                    }
                    //stringOutput.AppendLine(Properties.Resources.Done);
                    ProcessUITasks();
                }
            }

            CompileOutput.Text += stringOutput.ToString();
            CompileOutput.ScrollToEnd();

            if (CompileOutputRow.Height.Value < 11.0)
            {
                CompileOutputRow.Height = new GridLength(200.0);
            }

            InCompiling = false;
        }

        public static string ReformatCompOutput(string input)
        {
            StringBuilder output = new StringBuilder();
            foreach (string s in input.Split('\n').Where(s => !s.StartsWith("Copyright (c) ")))
            {
                output.Append(s.Trim('\r') + "\n");
            }
            return output.ToString().Trim('\r', '\n');
        }
 /*       public void Copy_Plugins(bool OvertakeOutString = false)
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
                            stringOutput.AppendLine(
                                string.Format(Properties.Resources.ErrorUploadFile, file, uploadDir));
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
        */

        public bool ServerIsRunning;
        public Process ServerProcess;
        public Thread ServerCheckThread;
        public string stdout = string.Empty;
        public StreamWriter serverStdIn;
        public bool result;


        public void Server_SendCmd(string command)
        {


            if (ServerIsRunning)
            {

                serverStdIn.WriteLine("status");
                //serverStdIn.WriteLine(command);
            }

        }
        public void Server_Start()
        {
            if (ServerIsRunning)
            {
                return;
            }

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
                            WorkingDirectory = @"D:\cs\csserver-rehlds",
                            //FileName = "cmd.exe",
                            UseShellExecute = false,
                            FileName = serverExec.FullName,
                            //WorkingDirectory = serverExec.DirectoryName,
                            Arguments = c.ServerArgs,
                            RedirectStandardOutput = true,
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            //CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
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
                serverStdIn = new StreamWriter(ServerProcess.StandardInput.BaseStream, Encoding.ASCII);
                ServerProcess.OutputDataReceived +=
                    (sender, args) =>
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            ServerOutput.Text += args.Data + Environment.NewLine;
                        });
                    };
                ServerProcess.StandardInput.WriteLine("status");
                
                ServerProcess.BeginOutputReadLine();
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
                    //ServerOutput.Text += stdout;
                    UpdateWindowTitle();
                }
            });
        }


        private string ExecuteCommandLine(string code, string directory, string copyDir, string scriptFile,
            string scriptName, string pluginFile, string pluginName)
        {
            code = ReplaceCMDVaraibles(code, directory, copyDir, scriptFile, scriptName, pluginFile, pluginName);
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            string batchFile = (new FileInfo(Path.Combine("sourcepawn\\temp\\",
                Environment.TickCount + "_" + ((uint) code.GetHashCode() ^ (uint) directory.GetHashCode()) +
                "_temp.bat"))).FullName;
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

        private string ReplaceCMDVaraibles(string CMD, string scriptDir, string copyDir, string scriptFile,
            string scriptName, string pluginFile, string pluginName)
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