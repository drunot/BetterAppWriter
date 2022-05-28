

#include"pch.h"
#include"CursorPosition.h"
#include<oleacc.h>
#include<Windows.h>
#include<WinUser.h>
#include<iostream>
#include<UIAutomationClient.h>
#include<atlbase.h>

screenPos getCursorPos() {
    screenPos ret = { 0, 0, 0, 0 };
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
    SetThreadDpiAwarenessContext(tempCOntext);
    
    if(SUCCEEDED(AccessibleObjectFromWindow(hwnd, OBJID_CARET, IID_IAccessible, (void**)&pAccCaret))) {
        hr = pAccCaret->accLocation(&ret.x, &ret.y, &ret.width, &ret.height, varCaret);
        
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
    
    SetThreadDpiAwarenessContext(normalDPIContext);
    return ret;
}

unsigned int getCurrentScale() {
    CoInitialize(NULL);
    GUITHREADINFO pgui;
    pgui.cbSize = sizeof(GUITHREADINFO);
    GetGUIThreadInfo(0, &pgui);
    HWND hwnd = pgui.hwndCaret == NULL ? pgui.hwndFocus : pgui.hwndCaret;
    DPI_AWARENESS_CONTEXT tempCOntext = GetWindowDpiAwarenessContext(hwnd);
    return GetDpiFromDpiAwarenessContext(tempCOntext);
}