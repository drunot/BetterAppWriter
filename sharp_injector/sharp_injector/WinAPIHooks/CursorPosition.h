#pragma once
extern "C" {
    struct screenPos {
        long x;
        long y;
        long width;
        long height;
    };
    
    __declspec(dllexport) screenPos getCursorPos();
    __declspec(dllexport) unsigned int getCurrentScale();
    __declspec(dllexport) unsigned int getMainScale();
}