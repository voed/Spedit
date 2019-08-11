using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Hjson;
using Newtonsoft.Json;
using SourcepawnCondenser.SourcemodDefinition;
using Spedit.Utils;

namespace Spedit.Interop
{
    public static class ConfigLoader
    {
        public static string ConfigPath = @"amxmodx\configs\configs.json";
        public static ConfigList Load()
        {
            try
            {
                using (StreamReader file = File.OpenText(ConfigPath))
                {
                    return (ConfigList) new JsonSerializer().Deserialize(file, typeof(ConfigList));
                }
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is DirectoryNotFoundException)
                {
                    MessageBox.Show(
                        $"Failed to load config file. Default config will be loaded{Environment.NewLine}Details: {e.Message}",
                        "Error while reading configs.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    //todo open config settings
                }

                MessageBox.Show(e.ToString());
            }
            
            return new ConfigList(){Configs = {new Config(){Name = "Default config"}}};
        }

        public static bool Save(ConfigList list)
        {
            using (StreamWriter file = File.CreateText(ConfigPath))
            {
                new JsonSerializer().Serialize(file, list);
            }

            return true;
        }
    }

    [JsonObject]
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

    public class ConfigList
    {

        public List<Config> Configs { get; set; }
        public int CurrentConfig { get; set; }

        [JsonIgnore]
        public Config Current {
            get => Configs[CurrentConfig];
            set => CurrentConfig = Configs.IndexOf(value);
        }

        public ConfigList()
        {
            Configs = new List<Config>();
        }
    }
}
