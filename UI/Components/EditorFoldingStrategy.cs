﻿using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Spedit.UI.Components
{
    public class SPFoldingStrategy
    {
        public char OpeningBrace { get; set; }
        public char ClosingBrace { get; set; }

        public SPFoldingStrategy()
		{
			OpeningBrace = '{';
			ClosingBrace = '}';
		}

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();

            Stack<int> startOffsets = new Stack<int>();
            int lastNewLineOffset = 0;
            bool InCommentMode = false;
            for (int i = 0; i < document.TextLength; ++i)
            {
                char c = document.GetCharAt(i);
                if (c == '\n' || c == '\r')
                {
                    lastNewLineOffset = i + 1;
                }
                else if (InCommentMode)
                {
                    if (c == '/')
                    {
                        if (i > 0)
                        {
                            if (document.GetCharAt(i - 1) == '*')
                            {
                                int startOffset = startOffsets.Pop();
                                InCommentMode = false;
                                if (startOffset < lastNewLineOffset)
                                {
                                    newFoldings.Add(new NewFolding(startOffset, i + 1));
                                }
                            }
                        }
                    }
                }
                else if (c == '/')
                {
                    if ((i + 1) < document.TextLength)
                    {
                        if (document.GetCharAt(i + 1) == '*')
                        {
                            InCommentMode = true;
                            startOffsets.Push(i);
                        }
                    }
                }
                else if (c == '{')
                {
                    startOffsets.Push(i);
                }
                else if (c == '}' && startOffsets.Count > 0)
                {
                    int startOffset = startOffsets.Pop();
                    if (startOffset < lastNewLineOffset)
                    {
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                    }
                }
            }

            /*Stack<int> startOffsets = new Stack<int>();
            int lastNewLineOffset = 0;
            char openingBrace = this.OpeningBrace;
            char closingBrace = this.ClosingBrace;
            for (int i = 0; i < document.TextLength; i++)
            {
                char c = document.GetCharAt(i);
                if (c == openingBrace)
                {
                    startOffsets.Push(i);
                }
                else if (c == closingBrace && startOffsets.Count > 0)
                {
                    int startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset)
                    {
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    lastNewLineOffset = i + 1;
                }
            }*/

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}
