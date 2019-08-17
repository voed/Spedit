using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Spedit //leave this here instead of .Interop because of reasons...
{
	[Serializable]
    public class OptionsControl
    {
        private const string OptionsFile = "options.dat";

        public bool Program_UseHardwareAcceleration = true;

        public bool Program_CheckForUpdates = true;

        public string Program_SelectedConfig = string.Empty;

        public bool Program_OpenCustomIncludes = false;
        public bool Program_OpenIncludesRecursively = false;

		public bool Program_DynamicISAC = true;

		public string Program_AccentColor = "Red";
		public string Program_Theme = "BaseDark";

		public string Program_ObjectBrowserDirectory = string.Empty;
		public double Program_ObjectbrowserWidth = 300.0;

        public bool UI_Animations = true;
        public bool UI_ShowToolBar = false;

        public bool Editor_WordWrap = false;
        public double Editor_FontSize = 16.0;
        public string Editor_FontFamily = "Consolas";
        public double Editor_ScrollLines = 4.0;
        public bool Editor_AgressiveIndentation = true;
        public bool Editor_ReformatLineAfterSemicolon = true;
        public bool Editor_ReplaceTabsToWhitespace = false;
		public bool Editor_AutoCloseBrackets = true;
		public bool Editor_AutoCloseStringChars = true;
		public bool Editor_ShowSpaces = false;
		public bool Editor_ShowTabs = false;
		public int Editor_IndentationSize = 4;
		public bool Editor_AutoSave = false;
		public int Editor_AutoSaveInterval = 5 * 60;

		public string[] LastOpenFiles = new string[0];

        public bool SH_HighlightDeprecateds = true;

		public string Language = string.Empty;

        public SerializeableColor SH_Comments = new SerializeableColor(0xFF, 0x57, 0xA6, 0x49);
        public SerializeableColor SH_CommentsMarker = new SerializeableColor(0xFF, 0xFF, 0x20, 0x20);
        public SerializeableColor SH_Strings = new SerializeableColor(0xFF, 0xF4, 0x6B, 0x6C);
        public SerializeableColor SH_PreProcessor = new SerializeableColor(0xFF, 0x7E, 0x7E, 0x7E);
        public SerializeableColor SH_Types = new SerializeableColor(0xFF, 0x28, 0x90, 0xB0); //56 9C D5
        public SerializeableColor SH_TypesValues = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SH_Keywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SH_ContextKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SH_Chars = new SerializeableColor(0xFF, 0xD6, 0x9C, 0x85);
        public SerializeableColor SH_UnkownFunctions = new SerializeableColor(0xFF, 0x45, 0x85, 0xC5);
        public SerializeableColor SH_Numbers = new SerializeableColor(0xFF, 0x97, 0x97, 0x97);
        public SerializeableColor SH_SpecialCharacters = new SerializeableColor(0xFF, 0x8F, 0x8F, 0x8F);
        public SerializeableColor SH_Deprecated = new SerializeableColor(0xFF, 0xFF, 0x00, 0x00);
        public SerializeableColor SH_Constants = new SerializeableColor(0xFF, 0xBC, 0x62, 0xC5);
        public SerializeableColor SH_Functions = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SH_Methods = new SerializeableColor(0xFF, 0x3B, 0xC6, 0x7E);

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
                catch (FileNotFoundException)
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
    public class SerializeableColor
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public SerializeableColor(byte a, byte r, byte g, byte b)
        {
            A = a; R = r; G = g; B = b;
        }

        public static implicit operator SerializeableColor(Color c)
        { return new SerializeableColor(c.A, c.R, c.G, c.B ); }
        public static implicit operator Color(SerializeableColor c)
        { return Color.FromArgb(c.A, c.R, c.G, c.B); }
    }

}
