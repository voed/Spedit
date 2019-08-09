using System;
using System.Text;
using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
		Token[] t;
		int position;
		int length;
		SMDefinition def;
		string source;
        string FileName;

		public Condenser(string sourceCode, string fileName)
		{
			t = Tokenizer.Tokenizer.TokenizeString(sourceCode, true).ToArray();
			position = 0;
			length = t.Length;
			def = new SMDefinition();
			source = sourceCode;
			if (fileName.EndsWith(".inc", StringComparison.InvariantCultureIgnoreCase))
			{
				fileName = fileName.Substring(0, fileName.Length - 4);
			}
			FileName = fileName;
		}

		public SMDefinition Condense()
		{
			Token ct;
			while ((ct = t[position]).Kind != TokenKind.EOF)
			{
				if (ct.Kind == TokenKind.FunctionIndicator)
				{
					int newIndex = ConsumeSMFunction();
					if (newIndex != -1)
					{
						position = newIndex + 1;
						continue;
					}
				}
				if (ct.Kind == TokenKind.Enum)
				{
					int newIndex = ConsumeSMEnum();
					if (newIndex != -1)
					{
						position = newIndex + 1;
						continue;
					}
				}
                if (ct.Kind == TokenKind.PrePocessorDirective)
				{
					int newIndex = ConsumeSMPPDirective();
					if (newIndex != -1)
					{
						position = newIndex + 1;
						continue;
					}
				}

                if (ct.Kind == TokenKind.Constant)
                {
                    int newIndex = ConsumeSMConstant();
                    if (newIndex != -1)
                    {
                        position = newIndex + 1;
                        continue;
                    }
                }

                ++position;
			}
			def.Sort();
			return def;
		}

		private int BacktraceTestForToken(int startPosition, TokenKind testKind, bool ignoreEol, bool ignoreOtherTokens)
		{
			for (int i = startPosition; i >= 0; --i)
			{
				if (t[i].Kind == testKind)
				{
					return i;
				}
				else if (ignoreOtherTokens)
				{
					continue;
				}
				else if (t[i].Kind == TokenKind.EOL && ignoreEol)
				{
					continue;
				}
				return -1;
			}
			return -1;
		}
		private int FortraceTestForToken(int startPosition, TokenKind testKind, bool ignoreEol, bool ignoreOtherTokens)
		{
			for (int i = startPosition; i < length; ++i)
			{
				if (t[i].Kind == testKind)
				{
					return i;
				}
				else if (ignoreOtherTokens)
				{
					continue;
				}
				else if (t[i].Kind == TokenKind.EOL && ignoreEol)
				{
					continue;
				}
				return -1;
			}
			return -1;
		}

        public static string TrimComments(string comment)
        {
            StringBuilder outString = new StringBuilder();
            string[] lines = comment.Split('\r', '\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                var line = (lines[i].Trim()).TrimStart('/', '*', ' ', '\t');
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (i > 0) { outString.AppendLine(); }

                    outString.Append(line.StartsWith("@param") ? FormatParamLineString(line) : line);
                }
            }
            return outString.ToString().Trim();
        }
		public static string TrimFullname(string name)
		{
			StringBuilder outString = new StringBuilder();
			string[] lines = name.Split('\r', '\n');
			for (int i = 0; i < lines.Length; ++i)
			{
				if (!string.IsNullOrWhiteSpace(lines[i]))
				{
					if (i > 0)
					{
						outString.Append(" ");
					}
					outString.Append(lines[i].Trim(' ', '\t'));
				}
			}
			return outString.ToString();
		}

		private static string FormatParamLineString(string line)
		{
			string[] split = line.Replace('\t', ' ').Split(new[] { ' ' }, 3);
			if (split.Length > 2)
			{
				return ("@param " + split[1]).PadRight(24, ' ') + " " + split[2].Trim(' ', '\t');
			}
			return line;
		}
	}
}
