﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Spedit //leave this here instead of .Interop because of reasons...
{
	[Serializable]
    public class OptionsControl
    {
        public static int SVersion = 11;
        public int Version = 11;

        public byte[] Program_CryptoKey;
		public bool Program_UseHardwareSalts = true;

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
        public bool UI_ShowToolBar;

        public bool Editor_WordWrap = false;
        public double Editor_FontSize = 16.0;
        public string Editor_FontFamily = "Consolas";
        public double Editor_ScrollLines = 4.0;
        public bool Editor_AgressiveIndentation = true;
        public bool Editor_ReformatLineAfterSemicolon = true;
        public bool Editor_ReplaceTabsToWhitespace;
		public bool Editor_AutoCloseBrackets = true;
		public bool Editor_AutoCloseStringChars = true;
		public bool Editor_ShowSpaces;
		public bool Editor_ShowTabs;
		public int Editor_IndentationSize = 4;
		public bool Editor_AutoSave;
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

        public void FillNullToDefaults()
        {
            if (Program_CryptoKey == null)
            {
                ReCreateCryptoKey();
            }
            if (SVersion > Version)
            {
				Program.ClearUpdateFiles();
				if (Version < 2)
				{
					UI_ShowToolBar = false;
				}
				if (Version < 3)
				{
					Editor_ReformatLineAfterSemicolon = true;
					Editor_ScrollLines = 4.0;
					Program_CheckForUpdates = true;
				}
				if (Version < 4)
				{
					Editor_ReplaceTabsToWhitespace = false;
				}
				if (Version < 5)
				{
					Program_DynamicISAC = true;
				}
				if (Version < 7)
				{
					Program_AccentColor = "Red";
					Program_Theme = "BaseDark";
					NormalizeSHColors();
				}
				if (Version < 8)
				{
					Editor_AutoCloseBrackets = true;
				}
				if (Version < 9)
				{
					Editor_AutoCloseStringChars = true;
					Editor_ShowSpaces = false;
					Editor_ShowTabs = false;
					Editor_IndentationSize = 4;
					Language = "";
					Program_ObjectBrowserDirectory = string.Empty;
					Program_ObjectbrowserWidth = 300.0;
					Editor_AutoSave = false;
					Editor_AutoSaveInterval = 5 * 60;
					ReCreateCryptoKey();
					Program.MakeRCCKAlert();
                }
                if (Version < 10)
                {
                    Program_UseHardwareSalts = true;
                }
                if (Version < 11)
                {
                    if (Program_AccentColor == "Cyan")
                        Program_AccentColor = "Blue";
                }
                //new Optionsversion - reset new fields to default
                Version = SVersion; //then Update Version afterwars
            }
        }

        public void ReCreateCryptoKey()
        {
            byte[] buffer = new byte[16];
            using (RNGCryptoServiceProvider cryptoRandomProvider = new RNGCryptoServiceProvider()) //generate global unique cryptokey
            {
                cryptoRandomProvider.GetBytes(buffer);
            }
            Program_CryptoKey = buffer;
        }

		private void NormalizeSHColors()
		{
			SH_Comments = new SerializeableColor(0xFF, 0x57, 0xA6, 0x49);
			SH_CommentsMarker = new SerializeableColor(0xFF, 0xFF, 0x20, 0x20);
			SH_Strings = new SerializeableColor(0xFF, 0xF4, 0x6B, 0x6C);
			SH_PreProcessor = new SerializeableColor(0xFF, 0x7E, 0x7E, 0x7E);
			SH_Types = new SerializeableColor(0xFF, 0x28, 0x90, 0xB0); //56 9C D5
			SH_TypesValues = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SH_Keywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SH_ContextKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SH_Chars = new SerializeableColor(0xFF, 0xD6, 0x9C, 0x85);
			SH_UnkownFunctions = new SerializeableColor(0xFF, 0x45, 0x85, 0xC5);
			SH_Numbers = new SerializeableColor(0xFF, 0x97, 0x97, 0x97);
			SH_SpecialCharacters = new SerializeableColor(0xFF, 0x8F, 0x8F, 0x8F);
			SH_Deprecated = new SerializeableColor(0xFF, 0xFF, 0x00, 0x00);
			SH_Constants = new SerializeableColor(0xFF, 0xBC, 0x62, 0xC5);
			SH_Functions = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SH_Methods = new SerializeableColor(0xFF, 0x3B, 0xC6, 0x7E);
		}
    }

    [Serializable]
    public class SerializeableColor
    {
        public SerializeableColor(byte a, byte r, byte g, byte b)
        {
            A = a; R = r; G = g; B = b;
        }
        public byte A;
        public byte R;
        public byte G;
        public byte B;
        public static implicit operator SerializeableColor(Color c)
        { return new SerializeableColor(c.A, c.R, c.G, c.B ); }
        public static implicit operator Color(SerializeableColor c)
        { return Color.FromArgb(c.A, c.R, c.G, c.B); }
    }

    public static class OptionsControlIOObject
    {
        public static void Save()
        {
            try
            {
                var formatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream("options_0.dat", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    formatter.Serialize(fileStream, Program.OptionsObject);
                    var test = Program.OptionsObject;
                }
            }
            catch (Exception) { }
        }

        public static OptionsControl Load(out bool programIsNew)
        {
            try
            {
                if (File.Exists("options_0.dat"))
                {
                    object deserializedOptionsObj;
                    var formatter = new BinaryFormatter();
                    using (FileStream fileStream = new FileStream("options_0.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        deserializedOptionsObj = formatter.Deserialize(fileStream);
                    }
                    OptionsControl oc = (OptionsControl)deserializedOptionsObj;
                    oc.FillNullToDefaults();
					programIsNew = false;
					return oc;
                }
            }
            catch (Exception) { }
            OptionsControl oco = new OptionsControl();
            oco.ReCreateCryptoKey();
#if DEBUG
			programIsNew = false;
#else
			ProgramIsNew = true;
#endif
			return oco;
        }
    }
}
