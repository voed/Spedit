using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml;
using SourcepawnCondenser.SourcemodDefinition;
using Spedit.Utils;

namespace Spedit.Interop
{
    public static class ConfigLoader
    {
        public static Config[] Load()
        {
            List<Config> configs = new List<Config>();
            if (File.Exists("sourcepawn\\configs\\Configs.xml"))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load("sourcepawn\\configs\\Configs.xml");
                    if (document.ChildNodes.Count < 1)
                    {
                        throw new Exception("No main 'Configurations' node.");
                    }
                    XmlNode mainNode = document.ChildNodes[0];
                    if (mainNode.ChildNodes.Count < 1)
                    {
                        throw new Exception("No 'config' nodes found.");
                    }
                    for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                    {
                        XmlNode node = mainNode.ChildNodes[i];
                        string name = ReadAttributeStringSafe(ref node, "Name", "UNKOWN CONFIG " + (i + 1));
                        string smDirectoryStr = ReadAttributeStringSafe(ref node, "SMDirectory");
                        string[] smDirectoriesSplitted = smDirectoryStr.Split(';');
                        List<string> SMDirs = new List<string>();
                        foreach (string dir in smDirectoriesSplitted)
                        {
                            string d = dir.Trim();
                            if (Directory.Exists(d))
                            {
                                SMDirs.Add(d);
                            }
                        }
                        string standard = ReadAttributeStringSafe(ref node, "Standard", "0");
                        bool isStandardConfig = standard != "0" && !string.IsNullOrWhiteSpace(standard);
                        string autoCopyStr = ReadAttributeStringSafe(ref node, "AutoCopy", "0");
                        bool autoCopy = autoCopyStr != "0" && !string.IsNullOrWhiteSpace(autoCopyStr);
                        string copyDirectory = ReadAttributeStringSafe(ref node, "CopyDirectory");
                        string serverFile = ReadAttributeStringSafe(ref node, "ServerFile");
                        string serverArgs = ReadAttributeStringSafe(ref node, "ServerArgs");
                        string postCmd = ReadAttributeStringSafe(ref node, "PostCmd");
                        string preCmd = ReadAttributeStringSafe(ref node, "PreCmd");
                        int optimizationLevel = 2, verboseLevel = 1;
                        int subValue;
                        if (int.TryParse(ReadAttributeStringSafe(ref node, "OptimizationLevel", "2"), out subValue))
                        {
                            optimizationLevel = subValue;
                        }
                        if (int.TryParse(ReadAttributeStringSafe(ref node, "VerboseLevel", "1"), out subValue))
                        {
                            verboseLevel = subValue;
                        }
                        bool deleteAfterCopy = false;
                        string deleteAfterCopyStr = ReadAttributeStringSafe(ref node, "DeleteAfterCopy", "0");
                        if (!(deleteAfterCopyStr == "0" || string.IsNullOrWhiteSpace(deleteAfterCopyStr)))
                        {
                            deleteAfterCopy = true;
                        }
                        string ftpHost = ReadAttributeStringSafe(ref node, "FTPHost", "ftp://localhost/");
                        string ftpUser = ReadAttributeStringSafe(ref node, "FTPUser");
                        string encryptedFTPPW = ReadAttributeStringSafe(ref node, "FTPPassword");
                        string ftppw = ManagedAES.Decrypt(encryptedFTPPW);
                        string ftpDir = ReadAttributeStringSafe(ref node, "FTPDir");
                        string rConEngineSourceStr = ReadAttributeStringSafe(ref node, "RConSourceEngine", "1");
                        bool rConEngineTypeSource = !(rConEngineSourceStr == "0" || string.IsNullOrWhiteSpace(rConEngineSourceStr));
                        string rConIp = ReadAttributeStringSafe(ref node, "RConIP", "127.0.0.1");
                        string rConPortStr = ReadAttributeStringSafe(ref node, "RConPort", "27015");
                        ushort rConPort;
                        if (!ushort.TryParse(rConPortStr, NumberStyles.Any, CultureInfo.InvariantCulture, out rConPort))
                        {
                            rConPort = 27015;
                        }
                        string encryptedRConPassword = ReadAttributeStringSafe(ref node, "RConPassword");
                        string rConPassword = ManagedAES.Decrypt(encryptedRConPassword);
                        string rConCommands = ReadAttributeStringSafe(ref node, "RConCommands");
                        Config c = new Config
                        {
                            Name = name,
                            SMDirectories = SMDirs.ToArray(),
                            Standard = isStandardConfig
                            ,
                            AutoCopy = autoCopy,
                            CopyDirectory = copyDirectory,
                            ServerFile = serverFile,
                            ServerArgs = serverArgs
                            ,
                            PostCmd = postCmd,
                            PreCmd = preCmd,
                            OptimizeLevel = optimizationLevel,
                            VerboseLevel = verboseLevel,
                            DeleteAfterCopy = deleteAfterCopy
                            ,
                            FTPHost = ftpHost,
                            FTPUser = ftpUser,
                            FTPPassword = ftppw,
                            FTPDir = ftpDir
                            ,
                            RConUseSourceEngine = rConEngineTypeSource,
                            RConIP = rConIp,
                            RConPort = rConPort,
                            RConPassword = rConPassword,
                            RConCommands = rConCommands
                        };
                        if (isStandardConfig)
                        {
                            c.LoadSMDef();
                        }
                        configs.Add(c);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error appeared while reading the configs. Without them, the editor wont start. Reinstall program!" + Environment.NewLine + "Details: " + e.Message
                        , "Error while reading configs."
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            else
            {
                MessageBox.Show("The Editor could not find the Configs.xml file. Without it, the editor wont start. Reinstall program.", "File not found.", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(Environment.ExitCode);
            }
            return configs.ToArray();
        }

        private static string ReadAttributeStringSafe(ref XmlNode node, string attributeName, string defaultValue = "")
        {
            for (int i = 0; i < node.Attributes?.Count; ++i)
            {
                if (node.Attributes[i].Name == attributeName)
                {
                    return node.Attributes[i].Value;
                }
            }
            return defaultValue;
        }
    }

    public class Config
    {
        public string Name = string.Empty;

        public bool Standard;

        public bool AutoCopy;

        public string[] SMDirectories = new string[0];
        public string CopyDirectory = string.Empty;
        public string ServerFile = string.Empty;
        public string ServerArgs = string.Empty;

        public string PostCmd = string.Empty;
        public string PreCmd = string.Empty;

        public bool DeleteAfterCopy;

        public int OptimizeLevel = 2;
        public int VerboseLevel = 1;

        public string FTPHost = "ftp://localhost/";
        public string FTPUser = string.Empty;
        public string FTPPassword = string.Empty; //securestring? No! Because it's saved in plaintext and if you want to keep it a secret, you shouldn't automaticly uploade it anyways...
        public string FTPDir = string.Empty;

        public bool RConUseSourceEngine = true;
        public string RConIP = "127.0.0.1";
        public ushort RConPort = 27015;
        public string RConPassword = string.Empty;
        public string RConCommands = string.Empty;

        private SMDefinition SMDef;

        public SMDefinition GetSMDef()
        {
            if (SMDef == null)
            {
                LoadSMDef();
            }
            return SMDef;
        }

        public void InvalidateSMDef()
        {
            SMDef = null;
        }

        public void LoadSMDef()
        {
            if (SMDef != null)
            {
                return;
            }
            try
            {
				SMDefinition def = new SMDefinition();
				def.AppendFiles(SMDirectories);
				SMDef = def;
            }
            catch (Exception)
            {
                SMDef = new SMDefinition(); //this could be dangerous...
            }
        }
    }
}
