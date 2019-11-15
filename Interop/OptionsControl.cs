using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Spedit //leave this here instead of .Interop because of reasons...
{
	[Serializable]
    public class OptionsControl
    {
        private const string OptionsFile = "options.dat";

        #region Program
        public bool UseHardwareAcceleration = true;
        public string SelectedConfig = string.Empty;
        public bool OpenCustomIncludes = false;
        public bool OpenIncludesRecursively = false;
        public bool DynamicIsac = true;
        public string ObjectBrowserDirectory = string.Empty;
		public double ObjectbrowserWidth = 300.0;
        #endregion

        #region Editor
        public bool WordWrap = false;
        public double FontSize = 16.0;
        public string FontFamily = "Consolas";
        public double ScrollLines = 4.0;
        public bool AgressiveIndentation = true;
        public bool ReformatLineAfterSemicolon = true;
        public bool ReplaceTabsToWhitespace = false;
        public bool AutoCloseBrackets = true;
        public bool AutoCloseStringChars = true;
        public bool ShowSpaces = false;
        public bool ShowTabs = false;
        public int IndentationSize = 4;
        public bool AutoSave = false;
        public int AutoSaveInterval = 5 * 60;
        #endregion


		public string[] LastOpenFiles = new string[0];
        public bool HighlightDeprecateds = true;
        public string Language = string.Empty;

        public HighColors EditorColors = new HighColors
        {
            Comments = new SpColor(0x57, 0xA6, 0x49),
            CommentsMarker = new SpColor(0xFF, 0x20, 0x20),
            Strings = new SpColor(0xF4, 0x6B, 0x6C),
            PreProcessor = new SpColor(0x7E, 0x7E, 0x7E),
            Types = new SpColor(0x28, 0x90, 0xB0),
            TypesValues = new SpColor(0x56, 0x9C, 0xD5),
            Keywords = new SpColor(0x56, 0x9C, 0xD5),
            ContextKeywords = new SpColor(0x56, 0x9C, 0xD5),
            Chars = new SpColor(0xD6, 0x9C, 0x85),
            UnkownFunctions = new SpColor(0x45, 0x85, 0xC5),
            Numbers = new SpColor(0x97, 0x97, 0x97),
            SpecialCharacters = new SpColor(0x8F, 0x8F, 0x8F),
            Deprecated = new SpColor(0xFF, 0x00, 0x00),
            Constants = new SpColor(0xBC, 0x62, 0xC5),
            Functions = new SpColor(0x56, 0x9C, 0xD5),
            Methods = new SpColor(0x3B, 0xC6, 0x7E)
        };


        public static void Save()
        {
            using (FileStream fileStream = new FileStream(OptionsFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                new BinaryFormatter().Serialize(fileStream, Program.Options);
            }
        }

        public static OptionsControl Load(out bool programIsNew)
        {
            if (File.Exists(OptionsFile))
            {
                //todo encrypt all file?
                programIsNew = false;
                try
                {
                    using (FileStream fileStream = new FileStream(OptionsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return (OptionsControl)new BinaryFormatter().Deserialize(fileStream);
                    }
                }
                catch (SerializationException)
                {
                    return new OptionsControl();
                }

            }

#if DEBUG
            programIsNew = false;
#else
            ProgramIsNew = true;
#endif
            return new OptionsControl();
        }
    }

    [Serializable]
    public class HighColors
    {
        public SpColor Comments { get; set; }
        public SpColor CommentsMarker { get; set; }
        public SpColor Strings { get; set; }
        public SpColor PreProcessor { get; set; }
        public SpColor Types { get; set; }
        public SpColor TypesValues { get; set; }
        public SpColor Keywords { get; set; }
        public SpColor ContextKeywords { get; set; }
        public SpColor Chars { get; set; }
        public SpColor UnkownFunctions { get; set; }
        public SpColor Numbers { get; set; }
        public SpColor SpecialCharacters { get; set; }
        public SpColor Deprecated { get; set; }
        public SpColor Constants { get; set; }
        public SpColor Functions { get; set; }
        public SpColor Methods { get; set; }
    }


    [Serializable]
    public class SpColor
    {
        private readonly byte _a;
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;

        public SpColor(byte a, byte r, byte g, byte b)
        {
            _a = a; _r = r; _g = g; _b = b;
        }

        public SpColor(byte r, byte g, byte b)
        {
            _a = 0xFF;
            _r = r;
            _g = g;
            _b = b;
        }

        public static implicit operator SpColor(Color c)
        {
            return new SpColor(c.A, c.R, c.G, c.B );
        }

        public static implicit operator Color(SpColor c)
        {
            return Color.FromArgb(c._a, c._r, c._g, c._b);
        }
    }

}
