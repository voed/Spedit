using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SourcepawnCondenser.SourcemodDefinition
{
	public class SMDefinition
	{
		public List<SMFunction> Functions = new List<SMFunction>();
		public List<SMEnum> Enums = new List<SMEnum>();
        public List<SMDefine> Defines = new List<SMDefine>();
		public List<SMConstant> Constants = new List<SMConstant>();

		public string[] FunctionStrings = new string[0];
        public string[] ConstantsStrings = new string[0]; //ATTENTION: THIS IS NOT THE LIST OF ALL CONSTANTS - IT INCLUDES MUCH MORE

		public void Sort()
		{
            Functions = Functions.Distinct(new SMFunctionComparer()).ToList();
				Functions.Sort((a, b) => string.Compare(a.Name, b.Name));
				//Enums = Enums.Distinct(new SMEnumComparer()).ToList(); //enums can have the same name but not be the same...
				Enums.Sort((a, b) => string.Compare(a.Name, b.Name));
                Defines = Defines.Distinct(new SMDefineComparer()).ToList();
				Defines.Sort((a, b) => string.Compare(a.Name, b.Name));
				Constants = Constants.Distinct(new SMConstantComparer()).ToList();
				Constants.Sort((a, b) => string.Compare(a.Name, b.Name));
            }

		public void AppendFiles(string[] paths)
		{
			foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.inc", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        FileInfo fInfo = new FileInfo(file);
                        Condenser subCondenser = new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name);
                        var subDefinition = subCondenser.Condense();
                        Functions.AddRange(subDefinition.Functions);
                        Enums.AddRange(subDefinition.Enums);
                        Defines.AddRange(subDefinition.Defines);
                        Constants.AddRange(subDefinition.Constants);
                    }
                }
            }
			Sort();
			ProduceStringArrays();
		}

		public void ProduceStringArrays()
		{
			FunctionStrings = new string[Functions.Count];
			for (int i = 0; i < Functions.Count; ++i)
			{
				FunctionStrings[i] = Functions[i].Name;
			}

			List<string> constantNames = Constants.Select(i => i.Name).ToList();
            foreach (var e in Enums) { constantNames.AddRange(e.Entries); }
            constantNames.AddRange(Defines.Select(i => i.Name));
            constantNames.Sort(string.Compare);
			ConstantsStrings = constantNames.ToArray();
        }

		public ACNode[] ProduceACNodes()
		{
            List<ACNode> nodes = new List<ACNode> {Capacity = Enums.Count + Constants.Count + Functions.Count};

            nodes.AddRange(ACNode.ConvertFromStringArray(FunctionStrings, true, "▲ "));
            nodes.AddRange(ACNode.ConvertFromStringArray(ConstantsStrings, false, "• "));
            nodes.Sort((a, b) => string.Compare(a.EntryName, b.EntryName));
            return nodes.ToArray();
		}
		public ACNode[] ProduceISNodes()
		{
			List<ACNode> nodes = new List<ACNode>();

            nodes = nodes.Distinct(new ACNodeEqualityComparer()).ToList();
            nodes.Sort((a, b) => string.Compare(a.EntryName, b.EntryName));
            return nodes.ToArray();
		}

		public void MergeDefinitions(SMDefinition def)
		{
            Functions.AddRange(def.Functions);
            Enums.AddRange(def.Enums);
            Defines.AddRange(def.Defines);
            Constants.AddRange(def.Constants);
        }

		public SMDefinition ProduceTemporaryExpandedDefinition(SMDefinition[] definitions)
		{
			SMDefinition def = new SMDefinition();

            def.MergeDefinitions(this);
            foreach (var definition in definitions)
            {
                if (definition != null)
                {
                    def.MergeDefinitions(definition);
                }
            }

            def.Sort();
            def.ProduceStringArrays();
            return def;
		}

		private class SMFunctionComparer : IEqualityComparer<SMFunction>
		{
			public bool Equals(SMFunction left, SMFunction right)
			{ return left?.Name == right?.Name; }

			public int GetHashCode(SMFunction sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMEnumComparer : IEqualityComparer<SMEnum>
		{
			public bool Equals(SMEnum left, SMEnum right)
			{ return left?.Name == right?.Name; }

			public int GetHashCode(SMEnum sm)
			{ return sm.Name.GetHashCode(); }
		}
        private class SMDefineComparer : IEqualityComparer<SMDefine>
		{
			public bool Equals(SMDefine left, SMDefine right)
			{ return left?.Name == right?.Name; }

			public int GetHashCode(SMDefine sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMConstantComparer : IEqualityComparer<SMConstant>
		{
			public bool Equals(SMConstant left, SMConstant right)
			{ return left?.Name == right?.Name; }

			public int GetHashCode(SMConstant sm)
			{ return sm.Name.GetHashCode(); }
		}

        public class ACNodeEqualityComparer : IEqualityComparer<ACNode>
        {
            public bool Equals(ACNode nodeA, ACNode nodeB)
            { return nodeA?.EntryName == nodeB?.EntryName; }

            public int GetHashCode(ACNode node)
            { return node.EntryName.GetHashCode(); }
        }

	}

	public class ACNode
	{
		public string Name;
		public string EntryName;
		public bool IsExecuteable;

		public static List<ACNode> ConvertFromStringArray(string[] strings, bool Executable, string prefix = "")
        {
            return strings.Select(str => new ACNode {Name = prefix + str, EntryName = str, IsExecuteable = Executable}).ToList();
        }

		public override string ToString()
		{
			return Name;
		}
	}
}
