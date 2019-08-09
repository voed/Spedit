﻿using SourcepawnCondenser;
using SourcepawnCondenser.SourcemodDefinition;
using System.Threading;
using System.Timers;
using System.IO;
using Spedit.UI.Components;

namespace Spedit.UI
{
	public partial class MainWindow
	{
		public ulong currentSMDefUID = 0;
		Thread backgroundParserThread;
		SMDefinition currentSMDef = null;
		System.Timers.Timer parseDistributorTimer;

		public void StartBackgroundParserThread()
		{
			backgroundParserThread = new Thread(new ThreadStart(BackgroundParser_Worker));
			backgroundParserThread.Start();
			parseDistributorTimer = new System.Timers.Timer(500.0);
			parseDistributorTimer.Elapsed += ParseDistributorTimer_Elapsed;
			parseDistributorTimer.Start();
		}

		private void ParseDistributorTimer_Elapsed(object sender, ElapsedEventArgs args)
		{
			if (currentSMDefUID == 0) { return; }
			EditorElement[] ee = null;
			EditorElement ce = null;
			Dispatcher.Invoke(() =>
			{
				ee = GetAllEditorElements();
				ce = GetCurrentEditorElement();
			});
			if (ee == null || ce == null) { return; } //this can happen!
			foreach (var e in ee)
			{
				if (e.LastSMDefUpdateUID < currentSMDefUID) //wants an update of the SMDefintion
				{
					if (e == ce)
					{
						if (ce.ISAC_Open)
						{
							continue;
						}
					}
					e.InterruptLoadAutoCompletes(currentSMDef.FunctionStrings, currentSMFunctions, currentACNodes, currentISNodes);
					e.LastSMDefUpdateUID = currentSMDefUID;
				}
			}
		}
		
		public SMFunction[] currentSMFunctions;
		public ACNode[] currentACNodes;
		public ISNode[] currentISNodes;

		private void BackgroundParser_Worker()
		{
			while (true)
			{
				while (Program.OptionsObject.Program_DynamicISAC)
				{
					Thread.Sleep(5000);
					var ee = GetAllEditorElements();
					if (ee != null)
					{
						SMDefinition[] definitions = new SMDefinition[ee.Length];
						for (int i = 0; i < ee.Length; ++i)
						{
							FileInfo fInfo = new FileInfo(ee[i].FullFilePath);
							if (fInfo.Extension.Trim('.').ToLowerInvariant() == "inc")
							{
								var condenser = new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name);
								definitions[i] = ((new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name)).Condense());
							}
						}
						currentSMDef = (Program.Configs[Program.SelectedConfig].GetSMDef()).ProduceTemporaryExpandedDefinition(definitions);
						currentSMFunctions = currentSMDef.Functions.ToArray();
						currentACNodes = currentSMDef.ProduceACNodes();
						currentISNodes = currentSMDef.ProduceISNodes();
						++currentSMDefUID;
					}
				}
				Thread.Sleep(5000);
			}
		}
	}
}
