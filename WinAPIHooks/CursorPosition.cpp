

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
    POINT ptZero = { 0, 0 };
    HMONITOR m = MonitorFromPoint(ptZero, MONITOR_DEFAULTTOPRIMARY);
    UINT ret = 0;
    UINT dpiY;
    HRESULT temp2 = GetDpiForMonitor(m, MDT_EFFECTIVE_DPI, &ret, &dpiY);
    SetThreadDpiAwarenessContext(normalDPIContext);
    return ret;
}

BOOL moveWinScaled(HWND wHnd, int x, int y, int width, int height) {
    DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
    SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
    auto ret = MoveWindow(wHnd, x, y, width, height, TRUE);
    SetThreadDpiAwarenessContext(normalDPIContext);
    return ret;
}

#include <fstream>

static constexpr int cursor_window_buffer = 20;

static bool reset = false;
static screenPos oldPos;
BOOL moveWinIfObstructing(HWND wHnd) {
    // Change DPI awareness to make everything easier!
    DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
    SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
    // Get cursor position.
    auto cursorPos = getCursorPos();
    BOOL cursorFound = cursorPos.width != 0.0 || cursorPos.height != 0.0;
    std::ofstream o;
    o.open("/Users/anton/Desktop/cpplog.log");
    o << "[" << __LINE__ << "] cursorFound: " <<  cursorFound << "\n";
    
    if(!cursorFound) {
        o.close();
        return false;
    }
    
    // Get window position
    RECT windowRect;
    GetWindowRect(wHnd, &windowRect);
    screenPos windowPos;
    windowPos.x = windowRect.left;
    windowPos.y = windowRect.top;
    windowPos.width = windowRect.right - windowRect.left;
    windowPos.height = windowRect.bottom - windowRect.top;
    
    if(!reset) {
        oldPos = windowPos;
        reset = true;
    }
    
    o << "[" << __LINE__ << "] IF Statement " << (oldPos.x < cursorPos.x &&
            (oldPos.x + oldPos.width) > cursorPos.x &&
            oldPos.y < cursorPos.y &&
            (oldPos.y + oldPos.height) > cursorPos.y) << "\n";
    o << "[" << __LINE__ << "] x top " << (oldPos.y < cursorPos.y) << "\n";
    o << "[" << __LINE__ << "] x bot " << ((oldPos.y + oldPos.height) > cursorPos.y) << "\n";
    o << "[" << __LINE__ << "] x left " << (oldPos.x < cursorPos.x) << "\n";
    o << "[" << __LINE__ << "] x right " << ((oldPos.x + oldPos.width) > cursorPos.x) << "\n";
    o << "[" << __LINE__ << "] oldPos.x " << oldPos.x << "\n";
    o << "[" << __LINE__ << "] oldPos.y " << oldPos.y << "\n";
    o << "[" << __LINE__ << "] oldPos.width " << oldPos.width << "\n";
    o << "[" << __LINE__ << "] oldPos.height " << oldPos.height << "\n";
    o << "[" << __LINE__ << "] cursorPos.x " << cursorPos.x << "\n";
    o << "[" << __LINE__ << "] cursorPos.y " << cursorPos.y << "\n";
    o << "[" << __LINE__ << "] cursorPos.width " << cursorPos.width << "\n";
    o << "[" << __LINE__ << "] cursorPos.height " << cursorPos.height << "\n";
    
    // If the cursor is within the bounds of the window.
    if(oldPos.x < cursorPos.x &&
            (oldPos.x + oldPos.width) > cursorPos.x &&
            oldPos.y < cursorPos.y &&
            (oldPos.y + oldPos.height) > cursorPos.y) {
        POINT ptWindow = { oldPos.x, oldPos.y };
        HMONITOR m = MonitorFromWindow(wHnd, MONITOR_DEFAULTTONEAREST);
        MONITORINFO mInfo;
        mInfo.cbSize = sizeof(MONITORINFO);
        GetMonitorInfo(m, &mInfo);
        o << "[" << __LINE__ << "] before if\n";
        // Place above if that is where it will be moved less
        // unless it ends out of the screen.
        o << "[" << __LINE__ << "] (cursorPos.y - oldPos.y) " << (cursorPos.y - oldPos.y) << "\n";
        o << "[" << __LINE__ << "] ((oldPos.y + oldPos.height) - (cursorPos.y + cursorPos.height)) " << ((oldPos.y + oldPos.height) - (cursorPos.y + cursorPos.height)) << "\n";
        
        if((cursorPos.y - oldPos.y) > ((oldPos.y + oldPos.height) - (cursorPos.y + cursorPos.height)) &&
                oldPos.y - (oldPos.height + cursor_window_buffer) > mInfo.rcMonitor.top) {
            o << "[" << __LINE__ << "] if\n";
            auto ret = MoveWindow(wHnd, oldPos.x, cursorPos.y - oldPos.height, oldPos.width, oldPos.height, TRUE);
            // Reset DPI awareness.
            SetThreadDpiAwarenessContext(normalDPIContext);
            o.close();
            return ret;
        } else {
            o << "[" << __LINE__ << "] else\n";
            auto ret = MoveWindow(wHnd, oldPos.x, cursorPos.y + cursorPos.height, oldPos.width, oldPos.height, TRUE);
            // Reset DPI awareness.
            SetThreadDpiAwarenessContext(normalDPIContext);
            o.close();
            return ret;
        }
    } else if(reset) {
        auto ret = MoveWindow(wHnd, oldPos.x, oldPos.y, oldPos.width, oldPos.height, TRUE);
        reset = false;
        // Reset DPI awareness.
        SetThreadDpiAwarenessContext(normalDPIContext);
        o.close();
        return ret;
    }
    
    o.close();
    return false;
}

void updateObstructingOldPos(HWND wHnd) {
    // Change DPI awareness to make everything easier!
    DPI_AWARENESS_CONTEXT normalDPIContext = GetThreadDpiAwarenessContext();
    SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
    // Get window position
    RECT windowRect;
    GetWindowRect(wHnd, &windowRect);
    screenPos windowPos;
    oldPos.x = windowRect.left;
    oldPos.y = windowRect.top;
    oldPos.width = windowRect.right - windowRect.left;
    oldPos.height = windowRect.bottom - windowRect.top;
    // Reset DPI awareness.
    SetThreadDpiAwarenessContext(normalDPIContext);
}