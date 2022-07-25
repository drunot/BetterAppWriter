

#include"pch.h"
#include"CursorPosition.h"
#include<oleacc.h>
#include<Windows.h>
#include<WinUser.h>
#include<iostream>
#include<UIAutomationClient.h>
#include<atlbase.h>
#include<ShellScalingAPI.h>

screenPos getCursorPos() {
    screenPos ret = { 0, 0, 0, 0 };
    // Do not know what CoInitialize does, but it makes position tracking much more reliable.
    CoInitialize(NULL);
    DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
    HRESULT hr;
    GUITHREADINFO pgui;
    pgui.cbSize = sizeof(GUITHREADINFO);
    IAccessible* pAccCaret = NULL;
    VARIANT varCaret;
    varCaret.vt = VT_I4;
    varCaret.lVal = CHILDID_SELF;
    GetGUIThreadInfo(0, &pgui);
    HWND hwnd = pgui.hwndCaret == NULL ? pgui.hwndFocus : pgui.hwndCaret;
    DPI_AWARENESS_CONTEXT tempCOntext = GetWindowDpiAwarenessContext(hwnd);
    // Right now there is a bug in the WinAPI where it uses the DPI to get the coordinates
    // To get the correct position, the DPI of this process must match the one of the window with the cursor.
    SetThreadDpiAwarenessContext(tempCOntext);
    
    if(SUCCEEDED(AccessibleObjectFromWindow(hwnd, OBJID_CARET, IID_IAccessible, (void**)&pAccCaret))) {
        // Use the most reliable strategy to get the coordinates.
        hr = pAccCaret->accLocation(&ret.x, &ret.y, &ret.width, &ret.height, varCaret);
        
        // If the reliable strategy fails attempt the other one even though it is unlikely that it then will work.
        if(ret.x == 0 && ret.y == 0 && ret.width == 0 && ret.height == 0) {
            if(pgui.rcCaret.left != 0 && pgui.rcCaret.bottom != 0) {
                POINT point;
                point.x = pgui.rcCaret.left;
                point.y = pgui.rcCaret.top;
                ClientToScreen(hwnd, &point);
                ret.x = point.x;
                ret.y = point.y;
                ret.width = pgui.rcCaret.right - pgui.rcCaret.left;
                ret.height = pgui.rcCaret.bottom - pgui.rcCaret.top;
            }
        }
        
        pAccCaret->Release();
    }
    
    // Set the DPI back again.
    SetThreadDpiAwarenessContext(normalDPIContext);
    return ret;
}

unsigned int getCurrentScale() {
    // Do not know what CoInitialize does, but it makes position tracking much more reliable.
    // Do not know if it is needed to get the DPI consistently.
    CoInitialize(NULL);
    GUITHREADINFO pgui;
    pgui.cbSize = sizeof(GUITHREADINFO);
    GetGUIThreadInfo(0, &pgui);
    HWND hwnd = pgui.hwndCaret == NULL ? pgui.hwndFocus : pgui.hwndCaret;
    DPI_AWARENESS_CONTEXT tempCOntext = GetWindowDpiAwarenessContext(hwnd);
    UINT ret = GetDpiFromDpiAwarenessContext(tempCOntext);
    
    if(ret == 0) {
        DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
        SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
        HMONITOR m = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        UINT dpiY;
        HRESULT temp2 = GetDpiForMonitor(m, MDT_EFFECTIVE_DPI, &ret, &dpiY);
        SetThreadDpiAwarenessContext(normalDPIContext);
    }
    
    return ret;
}


unsigned int getMainScale() {
    DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
    SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
    POINT ptZero = { 0,0 };
    HMONITOR m = MonitorFromPoint(ptZero, MONITOR_DEFAULTTOPRIMARY);
    UINT ret = 0;
    UINT dpiY;
    HRESULT temp2 = GetDpiForMonitor(m, MDT_EFFECTIVE_DPI, &ret, &dpiY);


    SetThreadDpiAwarenessContext(normalDPIContext);
    return ret;

}