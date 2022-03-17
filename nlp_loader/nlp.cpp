#include <windows.h>
#include <stdio.h>
#include "hook_macro.h"

HINSTANCE mHinst = 0, mHinstDLL = 0;

UINT_PTR mProcs[18] = {0};

LPCSTR mImportNames[] = {
  "ConvertTtsMarks",
  "ConvertTtsMarks_8",
  "Free",
  "Free_4",
  "FreeAbbreviations",
  "FreeAbbreviations_0",
  "GenerateSsml",
  "GenerateSsml_12",
  "GetPredictions",
  "GetPredictions_20",
  "InitializePrediction",
  "InitializePrediction_0",
  "LoadAbbreviations",
  "LoadAbbreviations_4",
  "ParseText",
  "ParseText_4",
  "UninitializePrediction",
  "UninitializePrediction_0",
};

#ifndef _DEBUG
inline void log_info(const char* info) {
}
#else
FILE* debug;
inline void log_info(const char* info) {
  fprintf(debug, "%s\n", info);
  fflush(debug);
}
#endif

#include "empty.h"

inline void _hook_setup() {
#ifdef CONVERTTTSMARKS
  ConvertTtsMarks_real = (ConvertTtsMarks_ptr)mProcs[0];
  mProcs[0] = (UINT_PTR)&ConvertTtsMarks_fake;
#endif
#ifdef CONVERTTTSMARKS_8
  ConvertTtsMarks_8_real = (ConvertTtsMarks@8_ptr)mProcs[1];
  mProcs[1] = (UINT_PTR)&ConvertTtsMarks_8_fake;
#endif
#ifdef FREE
  Free_real = (Free_ptr)mProcs[2];
  mProcs[2] = (UINT_PTR)&Free_fake;
#endif
#ifdef FREE_4
  Free_4_real = (Free@4_ptr)mProcs[3];
  mProcs[3] = (UINT_PTR)&Free_4_fake;
#endif
#ifdef FREEABBREVIATIONS
  FreeAbbreviations_real = (FreeAbbreviations_ptr)mProcs[4];
  mProcs[4] = (UINT_PTR)&FreeAbbreviations_fake;
#endif
#ifdef FREEABBREVIATIONS_0
  FreeAbbreviations_0_real = (FreeAbbreviations@0_ptr)mProcs[5];
  mProcs[5] = (UINT_PTR)&FreeAbbreviations_0_fake;
#endif
#ifdef GENERATESSML
  GenerateSsml_real = (GenerateSsml_ptr)mProcs[6];
  mProcs[6] = (UINT_PTR)&GenerateSsml_fake;
#endif
#ifdef GENERATESSML_12
  GenerateSsml_12_real = (GenerateSsml@12_ptr)mProcs[7];
  mProcs[7] = (UINT_PTR)&GenerateSsml_12_fake;
#endif
#ifdef GETPREDICTIONS
  GetPredictions_real = (GetPredictions_ptr)mProcs[8];
  mProcs[8] = (UINT_PTR)&GetPredictions_fake;
#endif
#ifdef GETPREDICTIONS_20
  GetPredictions_20_real = (GetPredictions@20_ptr)mProcs[9];
  mProcs[9] = (UINT_PTR)&GetPredictions_20_fake;
#endif
#ifdef INITIALIZEPREDICTION
  InitializePrediction_real = (InitializePrediction_ptr)mProcs[10];
  mProcs[10] = (UINT_PTR)&InitializePrediction_fake;
#endif
#ifdef INITIALIZEPREDICTION_0
  InitializePrediction_0_real = (InitializePrediction@0_ptr)mProcs[11];
  mProcs[11] = (UINT_PTR)&InitializePrediction_0_fake;
#endif
#ifdef LOADABBREVIATIONS
  LoadAbbreviations_real = (LoadAbbreviations_ptr)mProcs[12];
  mProcs[12] = (UINT_PTR)&LoadAbbreviations_fake;
#endif
#ifdef LOADABBREVIATIONS_4
  LoadAbbreviations_4_real = (LoadAbbreviations@4_ptr)mProcs[13];
  mProcs[13] = (UINT_PTR)&LoadAbbreviations_4_fake;
#endif
#ifdef PARSETEXT
  ParseText_real = (ParseText_ptr)mProcs[14];
  mProcs[14] = (UINT_PTR)&ParseText_fake;
#endif
#ifdef PARSETEXT_4
  ParseText_4_real = (ParseText@4_ptr)mProcs[15];
  mProcs[15] = (UINT_PTR)&ParseText_4_fake;
#endif
#ifdef UNINITIALIZEPREDICTION
  UninitializePrediction_real = (UninitializePrediction_ptr)mProcs[16];
  mProcs[16] = (UINT_PTR)&UninitializePrediction_fake;
#endif
#ifdef UNINITIALIZEPREDICTION_0
  UninitializePrediction_0_real = (UninitializePrediction@0_ptr)mProcs[17];
  mProcs[17] = (UINT_PTR)&UninitializePrediction_0_fake;
#endif
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved) {
  mHinst = hinstDLL;
  if (fdwReason == DLL_PROCESS_ATTACH) {
    mHinstDLL = LoadLibrary("real_nlp.dll");
    if (!mHinstDLL) {
      return FALSE;
    }
    for (int i = 0; i < 18; ++i) {
      mProcs[i] = (UINT_PTR)GetProcAddress(mHinstDLL, mImportNames[i]);
    }
    _hook_setup();
#ifdef _DEBUG
    debug = fopen("./debug.log", "a");
#endif
  } else if (fdwReason == DLL_PROCESS_DETACH) {
#ifdef _DEBUG
    fclose(debug);
#endif
    FreeLibrary(mHinstDLL);
  }
  return TRUE;
}

extern "C" __declspec(naked) void __stdcall ConvertTtsMarks_wrapper(){
#ifdef _DEBUG
  log_info("calling ConvertTtsMarks");
#endif
  __asm{jmp mProcs[0 * 4]}
}
#pragma comment(linker, "/EXPORT:ConvertTtsMarks=_ConvertTtsMarks_wrapper@0")
extern "C" __declspec(naked) void __stdcall ConvertTtsMarks_8_wrapper(){
#ifdef _DEBUG
  log_info("calling ConvertTtsMarks@8");
#endif
  __asm{jmp mProcs[1 * 4]}
}
#pragma comment(linker, "/EXPORT:ConvertTtsMarks@8=_ConvertTtsMarks_8_wrapper@0")
extern "C" __declspec(naked) void __stdcall Free_wrapper(){
#ifdef _DEBUG
  log_info("calling Free");
#endif
  __asm{jmp mProcs[2 * 4]}
}
#pragma comment(linker, "/EXPORT:Free=_Free_wrapper@0")
extern "C" __declspec(naked) void __stdcall Free_4_wrapper(){
#ifdef _DEBUG
  log_info("calling Free@4");
#endif
  __asm{jmp mProcs[3 * 4]}
}
#pragma comment(linker, "/EXPORT:Free@4=_Free_4_wrapper@0")
extern "C" __declspec(naked) void __stdcall FreeAbbreviations_wrapper(){
#ifdef _DEBUG
  log_info("calling FreeAbbreviations");
#endif
  __asm{jmp mProcs[4 * 4]}
}
#pragma comment(linker, "/EXPORT:FreeAbbreviations=_FreeAbbreviations_wrapper@0")
extern "C" __declspec(naked) void __stdcall FreeAbbreviations_0_wrapper(){
#ifdef _DEBUG
  log_info("calling FreeAbbreviations@0");
#endif
  __asm{jmp mProcs[5 * 4]}
}
#pragma comment(linker, "/EXPORT:FreeAbbreviations@0=_FreeAbbreviations_0_wrapper@0")
extern "C" __declspec(naked) void __stdcall GenerateSsml_wrapper(){
#ifdef _DEBUG
  log_info("calling GenerateSsml");
#endif
  __asm{jmp mProcs[6 * 4]}
}
#pragma comment(linker, "/EXPORT:GenerateSsml=_GenerateSsml_wrapper@0")
extern "C" __declspec(naked) void __stdcall GenerateSsml_12_wrapper(){
#ifdef _DEBUG
  log_info("calling GenerateSsml@12");
#endif
  __asm{jmp mProcs[7 * 4]}
}
#pragma comment(linker, "/EXPORT:GenerateSsml@12=_GenerateSsml_12_wrapper@0")
extern "C" __declspec(naked) void __stdcall GetPredictions_wrapper(){
#ifdef _DEBUG
  log_info("calling GetPredictions");
#endif
  __asm{jmp mProcs[8 * 4]}
}
#pragma comment(linker, "/EXPORT:GetPredictions=_GetPredictions_wrapper@0")
extern "C" __declspec(naked) void __stdcall GetPredictions_20_wrapper(){
#ifdef _DEBUG
  log_info("calling GetPredictions@20");
#endif
  __asm{jmp mProcs[9 * 4]}
}
#pragma comment(linker, "/EXPORT:GetPredictions@20=_GetPredictions_20_wrapper@0")
extern "C" __declspec(naked) void __stdcall InitializePrediction_wrapper(){
#ifdef _DEBUG
  log_info("calling InitializePrediction");
#endif
  __asm{jmp mProcs[10 * 4]}
}
#pragma comment(linker, "/EXPORT:InitializePrediction=_InitializePrediction_wrapper@0")
extern "C" __declspec(naked) void __stdcall InitializePrediction_0_wrapper(){
#ifdef _DEBUG
  log_info("calling InitializePrediction@0");
#endif
  __asm{jmp mProcs[11 * 4]}
}
#pragma comment(linker, "/EXPORT:InitializePrediction@0=_InitializePrediction_0_wrapper@0")
extern "C" __declspec(naked) void __stdcall LoadAbbreviations_wrapper(){
#ifdef _DEBUG
  log_info("calling LoadAbbreviations");
#endif
  __asm{jmp mProcs[12 * 4]}
}
#pragma comment(linker, "/EXPORT:LoadAbbreviations=_LoadAbbreviations_wrapper@0")
extern "C" __declspec(naked) void __stdcall LoadAbbreviations_4_wrapper(){
#ifdef _DEBUG
  log_info("calling LoadAbbreviations@4");
#endif
  __asm{jmp mProcs[13 * 4]}
}
#pragma comment(linker, "/EXPORT:LoadAbbreviations@4=_LoadAbbreviations_4_wrapper@0")
extern "C" __declspec(naked) void __stdcall ParseText_wrapper(){
#ifdef _DEBUG
  log_info("calling ParseText");
#endif
  __asm{jmp mProcs[14 * 4]}
}
#pragma comment(linker, "/EXPORT:ParseText=_ParseText_wrapper@0")
extern "C" __declspec(naked) void __stdcall ParseText_4_wrapper(){
#ifdef _DEBUG
  log_info("calling ParseText@4");
#endif
  __asm{jmp mProcs[15 * 4]}
}
#pragma comment(linker, "/EXPORT:ParseText@4=_ParseText_4_wrapper@0")
extern "C" __declspec(naked) void __stdcall UninitializePrediction_wrapper(){
#ifdef _DEBUG
  log_info("calling UninitializePrediction");
#endif
  __asm{jmp mProcs[16 * 4]}
}
#pragma comment(linker, "/EXPORT:UninitializePrediction=_UninitializePrediction_wrapper@0")
extern "C" __declspec(naked) void __stdcall UninitializePrediction_0_wrapper(){
#ifdef _DEBUG
  log_info("calling UninitializePrediction@0");
#endif
  __asm{jmp mProcs[17 * 4]}
}
#pragma comment(linker, "/EXPORT:UninitializePrediction@0=_UninitializePrediction_0_wrapper@0")
