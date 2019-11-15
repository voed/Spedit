
// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <cstdio>
#include "API/engine_hlds_api.h"
#include <string>
#include <Psapi.h>


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

IDedicatedServerAPI* engineAPI = nullptr;
CreateInterfaceFn engineFactory = nullptr;




EXTERN_C __declspec(dllexport) int hlds_command(char* text, HANDLE hProcess)
{
	//SetDllDirectory("D:\\cs\\csserver-rehlds");

	HMODULE hMods[1024];
	DWORD cbNeeded;
	HMODULE engine = nullptr;

	if (EnumProcessModules(hProcess, hMods, sizeof(hMods), &cbNeeded))
	{
		for (unsigned int i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
		{
			TCHAR szModName[MAX_PATH];

			// Get the full path to the module's file.

			if (GetModuleFileNameEx(hProcess, hMods[i], szModName, sizeof szModName / sizeof(TCHAR))
				&& std::basic_string<char>(szModName) == "D:\\cs\\csserver-rehlds\\swds.dll")
			{
				engine = hMods[i];
				break;
				//_tprintf( TEXT("\t%s (0x%08X)\n"), szModName, hMods[i] );
			}
		}
	}

	if (!engineFactory)
		//engineFactory = Sys_GetFactory(Sys_LoadModule("swds.dll"));
		engineFactory = Sys_GetFactory(reinterpret_cast<CSysModule*>(engine));
		if (engineFactory)
			engineAPI = reinterpret_cast<IDedicatedServerAPI*>(engineFactory(VENGINE_HLDS_API_VERSION, nullptr));

	if (engineAPI)
	{
		engineAPI->AddConsoleText(text);
	}
	return 0;
}

