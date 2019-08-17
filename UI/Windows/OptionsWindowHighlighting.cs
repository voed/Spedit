using System.Windows;

namespace Spedit.UI.Windows
{
    public partial class OptionsWindow
    {
        private void LoadSH()
        {
            SH_Comments.SetContent("Comments", Program.Options.SH_Comments);
            SH_CommentMarkers.SetContent("Comment Markers", Program.Options.SH_CommentsMarker);
            SH_PreProcessor.SetContent("Pre-Processor Directives", Program.Options.SH_PreProcessor);
            SH_Strings.SetContent("Strings / Includes", Program.Options.SH_Strings);
            SH_Types.SetContent("Types", Program.Options.SH_Types);
            SH_TypesValues.SetContent("Type-Values", Program.Options.SH_TypesValues);
            SH_Keywords.SetContent("Keywords", Program.Options.SH_Keywords);
            SH_ContextKeywords.SetContent("Context-Keywords", Program.Options.SH_ContextKeywords);
            SH_Chars.SetContent("Chars", Program.Options.SH_Chars);
            SH_Numbers.SetContent("Numbers", Program.Options.SH_Numbers);
            SH_SpecialCharacters.SetContent("Special Characters", Program.Options.SH_SpecialCharacters);
            SH_UnknownFunctions.SetContent("Unknown Functions", Program.Options.SH_UnkownFunctions);
            SH_Deprecated.SetContent("Deprecated Content", Program.Options.SH_Deprecated);
            SH_Constants.SetContent("Parsed Contants", Program.Options.SH_Constants);
            SH_Functions.SetContent("Parsed Functions", Program.Options.SH_Functions);
            SH_Methods.SetContent("Parsed Methods", Program.Options.SH_Methods);
        }

        private void Comments_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Comments = SH_Comments.GetColor();
            ToggleRestartText();
        }

        private void CommentMarker_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_CommentsMarker = SH_CommentMarkers.GetColor();
            ToggleRestartText();
        }

        private void PreProcessor_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_PreProcessor = SH_PreProcessor.GetColor();
            ToggleRestartText();
        }

        private void String_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Strings = SH_Strings.GetColor();
            ToggleRestartText();
        }

        private void Types_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Types = SH_Types.GetColor();
            ToggleRestartText();
        }

        private void TypeValues_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_TypesValues = SH_TypesValues.GetColor();
            ToggleRestartText();
        }

        private void Keywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Keywords = SH_Keywords.GetColor();
            ToggleRestartText();
        }

        private void ContextKeywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_ContextKeywords = SH_ContextKeywords.GetColor();
            ToggleRestartText();
        }
        
        private void Chars_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Chars = SH_Chars.GetColor();
            ToggleRestartText();
        }
        private void UFunctions_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_UnkownFunctions = SH_UnknownFunctions.GetColor();
            ToggleRestartText();
        }
        private void Numbers_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Numbers = SH_Numbers.GetColor();
            ToggleRestartText();
        }
        private void SpecialCharacters_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_SpecialCharacters = SH_SpecialCharacters.GetColor();
            ToggleRestartText();
        }
        private void Deprecated_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Deprecated = SH_Deprecated.GetColor();
            ToggleRestartText();
        }
        private void Constants_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Constants = SH_Constants.GetColor();
            ToggleRestartText();
        }
        private void Functions_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Functions = SH_Functions.GetColor();
            ToggleRestartText();
        }
        private void Methods_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_Methods = SH_Methods.GetColor();
            ToggleRestartText();
        }
    }
}
