﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReHLDSTest
{
    public static class CharArrayExtensions
    {
        public static char[] SubArray(this char[] input,int startIndex, int length)
        {
            List<char> result= new List<char>();
            for (int i = startIndex; i < length; i++)
            {
                result.Add(input[i]);
            }

            return result.ToArray();
        }
    }

    public class ConsoleAppManager
{
    private readonly string appName;
    private readonly Process process = new Process();
    private readonly object theLock = new object();
    private SynchronizationContext context;
    private string pendingWriteData;

    public ConsoleAppManager(string appName)
    {
        this.appName = appName;

        this.process.StartInfo.FileName = @"D:\cs\csserver-rehlds\hlds.exe";//this.appName;
        this.process.StartInfo.WorkingDirectory = @"D:\cs\csserver-rehlds";

            this.process.StartInfo.RedirectStandardError = true;
        this.process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

        this.process.StartInfo.RedirectStandardInput = true;
        this.process.StartInfo.RedirectStandardOutput = true;
        this.process.EnableRaisingEvents = true;
        this.process.StartInfo.CreateNoWindow = true;

        this.process.StartInfo.UseShellExecute = false;

        this.process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

        this.process.Exited += this.ProcessOnExited;
    }

    public event EventHandler<string> ErrorTextReceived;
    public event EventHandler ProcessExited;
    public event EventHandler<string> StandartTextReceived;

    public int ExitCode
    {
        get { return this.process.ExitCode; }
    }

    public bool Running
    {
        get; private set;
    }

    public void ExecuteAsync(params string[] args)
    {
        if (this.Running)
        {
            throw new InvalidOperationException(
                "Process is still Running. Please wait for the process to complete.");
        }

        string arguments = string.Join(" ", args);

        this.process.StartInfo.Arguments = arguments;

        this.context = SynchronizationContext.Current;

        this.process.Start();
        this.Running = true;

        new Task(this.ReadOutputAsync).Start();
        new Task(this.WriteInputTask).Start();
        new Task(this.ReadOutputErrorAsync).Start();
    }

    public void Write(string data)
    {
        if (data == null)
        {
            return;
        }

        lock (this.theLock)
        {
            this.pendingWriteData = data;
        }
    }

    public void WriteLine(string data)
    {
        this.Write(data + Environment.NewLine);
    }

    protected virtual void OnErrorTextReceived(string e)
    {
        EventHandler<string> handler = this.ErrorTextReceived;

        if (handler != null)
        {
            if (this.context != null)
            {
                this.context.Post(delegate { handler(this, e); }, null);
            }
            else
            {
                handler(this, e);
            }
        }
    }

    protected virtual void OnProcessExited()
    {
        EventHandler handler = this.ProcessExited;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    protected virtual void OnStandartTextReceived(string e)
    {
        EventHandler<string> handler = this.StandartTextReceived;

        if (handler != null)
        {
            if (this.context != null)
            {
                this.context.Post(delegate { handler(this, e); }, null);
            }
            else
            {
                handler(this, e);
            }
        }
    }

    private void ProcessOnExited(object sender, EventArgs eventArgs)
    {
        this.OnProcessExited();
    }

    private async void ReadOutputAsync()
    {
        var standart = new StringBuilder();
        var buff = new char[1024];
        int length;

        while (this.process.HasExited == false)
        {
            standart.Clear();

            length = await this.process.StandardOutput.ReadAsync(buff, 0, buff.Length);
            standart.Append(buff.SubArray(0, length) );
            this.OnStandartTextReceived(standart.ToString());
            Thread.Sleep(1);
        }

        this.Running = false;
    }

    private async void ReadOutputErrorAsync()
    {
        var sb = new StringBuilder();

        do
        {
            sb.Clear();
            var buff = new char[1024];
            int length = await this.process.StandardError.ReadAsync(buff, 0, buff.Length);
            sb.Append(buff.SubArray(0, length));
            this.OnErrorTextReceived(sb.ToString());
            Thread.Sleep(1);
        }
        while (this.process.HasExited == false);
    }

    private async void WriteInputTask()
    {
        while (this.process.HasExited == false)
        {
            Thread.Sleep(1);

            if (this.pendingWriteData != null)
            {
                await this.process.StandardInput.WriteLineAsync(this.pendingWriteData);
                await this.process.StandardInput.FlushAsync();

                lock (this.theLock)
                {
                    this.pendingWriteData = null;
                }
            }
        }
    }

}
    class Program
    {

        [DllImport("HLDSHelper.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int hlds_command([MarshalAs(UnmanagedType.LPStr)] string text, IntPtr process);

        static void Main(string[] args)
        {
            var hlds = new ConsoleAppManager("hlds");
            hlds.ExecuteAsync("-game cstrike -port 27016 +map de_dust2");
            hlds.WriteLine("status");
            
            hlds.StandartTextReceived += Hlds_StandartTextReceived;
            Console.ReadLine();
        }

        private static void Hlds_StandartTextReceived(object sender, string e)
        {
            Console.WriteLine(e);
        }

        static IntPtr findProcess()
        {
            return Process.GetProcessesByName("hlds").FirstOrDefault(x => x.MainModule.FileName.Equals(@"D:\cs\csserver-rehlds\hlds.exe")).Handle;
        }
        
    }

}
