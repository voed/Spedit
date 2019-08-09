namespace SourcepawnCondenser.Tokenizer
{
	public enum TokenKind
	{
		Identifier,				//done
		Number,					//d
		Character,				//d
		BraceOpen,				//d
		BraceClose,				//d
		ParenthesisOpen,		//d
		ParenthesisClose,		//d
		Quote,					//d
		SingleLineComment,		//d
		MultiLineComment,		//d
		Semicolon,				//d
		Comma,					//d
		Assignment,				//d
		
		FunctionIndicator,		//d
		Constant,				//d
		Enum,					//d
        Property,				//d
		PrePocessorDirective,	//d

        EOL,					//d
		EOF,					//d
	}
}
