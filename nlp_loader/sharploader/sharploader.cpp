#include <Windows.h>
#include <mscoree.h>
#include <metahost.h>
#include <wchar.h>
#include <fstream>
#include "sharploader.hpp"

#pragma comment(lib, "mscoree.lib")

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

static const LPCWSTR Assembly = L"\\sharp_injector.dll";
static const LPCWSTR Class    = L"sharp_injector.Startup";
static const LPCWSTR Method   = L"EntryPoint";
static const LPCWSTR Param    = L"";


DWORD WINAPI CreateDotNetRunTime(LPVOID lpParam) {
#ifdef _DEBUG
    auto file = fopen("\\Users\\anton\\Desktop\\nlp_loader.log", "a");
#endif
    ICLRRuntimeHost* lpRuntimeHost = NULL;
    ICLRRuntimeInfo* lpRuntimeInfo = NULL;
    ICLRMetaHost* lpMetaHost = NULL;
    LPWSTR AppPath = new WCHAR[_MAX_PATH];
    ::GetModuleFileNameW((HINSTANCE)&__ImageBase, AppPath, _MAX_PATH);
    std::wstring tempPath = AppPath;
    int index = tempPath.rfind('\\');
    tempPath.erase(index, tempPath.length() - index);
    tempPath += Assembly;
    HRESULT hr = CLRCreateInstance(
                     CLSID_CLRMetaHost,
                     IID_ICLRMetaHost,
                     (LPVOID*)&lpMetaHost
                 );
                 
    if(FAILED(hr)) {
#ifdef _DEBUG
        fprintf(file, "Failed to create CLR instance.\n");
        fflush(file);
#endif
        return hr;
    }
    
    hr = lpMetaHost->GetRuntime(
             L"v4.0.30319",
             IID_PPV_ARGS(&lpRuntimeInfo)
         );
         
    if(FAILED(hr)) {
#ifdef _DEBUG
        fprintf(file, "Getting runtime failed.\n");
        fflush(file);
#endif
        lpMetaHost->Release();
        return hr;
    }
    
    BOOL fLoadable;
    hr = lpRuntimeInfo->IsLoadable(&fLoadable);
    
    if(FAILED(hr) || !fLoadable) {
#ifdef _DEBUG
        fprintf(file, "Runtime can't be loaded into the process.\n");
        fflush(file);
#endif
        lpRuntimeInfo->Release();
        lpMetaHost->Release();
        return hr;
    }
    
    hr = lpRuntimeInfo->GetInterface(
             CLSID_CLRRuntimeHost,
             IID_PPV_ARGS(&lpRuntimeHost)
         );
         
    if(FAILED(hr)) {
#ifdef _DEBUG
        fprintf(file, "Failed to acquire CLR runtime.\n");
        fflush(file);
#endif
        lpRuntimeInfo->Release();
        lpMetaHost->Release();
        return hr;
    }
    
    hr = lpRuntimeHost->Start();
    
    if(FAILED(hr)) {
#ifdef _DEBUG
        fprintf(file, "Failed to start CLR runtime.\n");
        fflush(file);
#endif
        lpRuntimeHost->Release();
        lpRuntimeInfo->Release();
        lpMetaHost->Release();
        return hr;
    }
    
    DWORD dwRetCode = 0;
    hr = lpRuntimeHost->ExecuteInDefaultAppDomain(
             (LPWSTR)tempPath.c_str(),
             Class,
             Method,
             Param,
             &dwRetCode
         );
         
    if(FAILED(hr)) {
#ifdef _DEBUG
        fprintf(file, "Unable to execute assembly.\n");
        fflush(file);
#endif
        lpRuntimeHost->Stop();
        lpRuntimeHost->Release();
        lpRuntimeInfo->Release();
        lpMetaHost->Release();
        return hr;
    }
    
#ifdef _DEBUG
    fclose(file);
#endif
    return 0;
}