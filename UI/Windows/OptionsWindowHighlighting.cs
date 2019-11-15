using System.Windows;

namespace Spedit.UI.Windows
{
    public partial class OptionsWindow
    {
        private void LoadSH()
        {
            SH_Comments.SetContent("Comments", Program.Options.EditorColors.Comments);
            SH_CommentMarkers.SetContent("Comment Markers", Program.Options.EditorColors.CommentsMarker);
            SH_PreProcessor.SetContent("Pre-Processor Directives", Program.Options.EditorColors.PreProcessor);
            SH_Strings.SetContent("Strings / Includes", Program.Options.EditorColors.Strings);
            SH_Types.SetContent("Types", Program.Options.EditorColors.Types);
            SH_TypesValues.SetContent("Type-Values", Program.Options.EditorColors.TypesValues);
            SH_Keywords.SetContent("Keywords", Program.Options.EditorColors.Keywords);
            SH_ContextKeywords.SetContent("Context-Keywords", Program.Options.EditorColors.ContextKeywords);
            SH_Chars.SetContent("Chars", Program.Options.EditorColors.Chars);
            SH_Numbers.SetContent("Numbers", Program.Options.EditorColors.Numbers);
            SH_SpecialCharacters.SetContent("Special Characters", Program.Options.EditorColors.SpecialCharacters);
            SH_UnknownFunctions.SetContent("Unknown Functions", Program.Options.EditorColors.UnkownFunctions);
            SH_Deprecated.SetContent("Deprecated Content", Program.Options.EditorColors.Deprecated);
            SH_Constants.SetContent("Parsed Contants", Program.Options.EditorColors.Constants);
            SH_Functions.SetContent("Parsed Functions", Program.Options.EditorColors.Functions);
            SH_Methods.SetContent("Parsed Methods", Program.Options.EditorColors.Methods);
        }

        private void Comments_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Comments = SH_Comments.GetColor();
            ToggleRestartText();
        }

        private void CommentMarker_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.CommentsMarker = SH_CommentMarkers.GetColor();
            ToggleRestartText();
        }

        private void PreProcessor_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.PreProcessor = SH_PreProcessor.GetColor();
            ToggleRestartText();
        }

        private void String_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Strings = SH_Strings.GetColor();
            ToggleRestartText();
        }

        private void Types_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Types = SH_Types.GetColor();
            ToggleRestartText();
        }

        private void TypeValues_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.TypesValues = SH_TypesValues.GetColor();
            ToggleRestartText();
        }

        private void Keywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Keywords = SH_Keywords.GetColor();
            ToggleRestartText();
        }

        private void ContextKeywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.ContextKeywords = SH_ContextKeywords.GetColor();
            ToggleRestartText();
        }
        
        private void Chars_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Chars = SH_Chars.GetColor();
            ToggleRestartText();
        }
        private void UFunctions_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.UnkownFunctions = SH_UnknownFunctions.GetColor();
            ToggleRestartText();
        }
        private void Numbers_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Numbers = SH_Numbers.GetColor();
            ToggleRestartText();
        }
        private void SpecialCharacters_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.SpecialCharacters = SH_SpecialCharacters.GetColor();
            ToggleRestartText();
        }
        private void Deprecated_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Deprecated = SH_Deprecated.GetColor();
            ToggleRestartText();
        }
        private void Constants_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Constants = SH_Constants.GetColor();
            ToggleRestartText();
        }
        private void Functions_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Functions = SH_Functions.GetColor();
            ToggleRestartText();
        }
        private void Methods_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.EditorColors.Methods = SH_Methods.GetColor();
            ToggleRestartText();
        }
    }
}
