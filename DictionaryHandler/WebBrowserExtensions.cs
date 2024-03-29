﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Controls;

namespace DictionaryHandler {
    public static class WebBrowserExtensions {
        enum OLECMDID {
            OLECMDID_OPEN = 1,
            OLECMDID_NEW = 2,
            OLECMDID_SAVE = 3,
            OLECMDID_SAVEAS = 4,
            OLECMDID_SAVECOPYAS = 5,
            OLECMDID_PRINT = 6,
            OLECMDID_PRINTPREVIEW = 7,
            OLECMDID_PAGESETUP = 8,
            OLECMDID_SPELL = 9,
            OLECMDID_PROPERTIES = 10,
            OLECMDID_CUT = 11,
            OLECMDID_COPY = 12,
            OLECMDID_PASTE = 13,
            OLECMDID_PASTESPECIAL = 14,
            OLECMDID_UNDO = 15,
            OLECMDID_REDO = 16,
            OLECMDID_SELECTALL = 17,
            OLECMDID_CLEARSELECTION = 18,
            OLECMDID_ZOOM = 19,
            OLECMDID_GETZOOMRANGE = 20,
            OLECMDID_UPDATECOMMANDS = 21,
            OLECMDID_REFRESH = 22,
            OLECMDID_STOP = 23,
            OLECMDID_HIDETOOLBARS = 24,
            OLECMDID_SETPROGRESSMAX = 25,
            OLECMDID_SETPROGRESSPOS = 26,
            OLECMDID_SETPROGRESSTEXT = 27,
            OLECMDID_SETTITLE = 28,
            OLECMDID_SETDOWNLOADSTATE = 29,
            OLECMDID_STOPDOWNLOAD = 30,
            OLECMDID_ONTOOLBARACTIVATED = 31,
            OLECMDID_FIND = 32,
            OLECMDID_DELETE = 33,
            OLECMDID_HTTPEQUIV = 34,
            OLECMDID_HTTPEQUIV_DONE = 35,
            OLECMDID_ENABLE_INTERACTION = 36,
            OLECMDID_ONUNLOAD = 37,
            OLECMDID_PROPERTYBAG2 = 38,
            OLECMDID_PREREFRESH = 39,
            OLECMDID_SHOWSCRIPTERROR = 40,
            OLECMDID_SHOWMESSAGE = 41,
            OLECMDID_SHOWFIND = 42,
            OLECMDID_SHOWPAGESETUP = 43,
            OLECMDID_SHOWPRINT = 44,
            OLECMDID_CLOSE = 45,
            OLECMDID_ALLOWUILESSSAVEAS = 46,
            OLECMDID_DONTDOWNLOADCSS = 47,
            OLECMDID_UPDATEPAGESTATUS = 48,
            OLECMDID_PRINT2 = 49,
            OLECMDID_PRINTPREVIEW2 = 50,
            OLECMDID_SETPRINTTEMPLATE = 51,
            OLECMDID_GETPRINTTEMPLATE = 52,
            OLECMDID_PAGEACTIONBLOCKED = 55,
            OLECMDID_PAGEACTIONUIQUERY = 56,
            OLECMDID_FOCUSVIEWCONTROLS = 57,
            OLECMDID_FOCUSVIEWCONTROLSQUERY = 58,
            OLECMDID_SHOWPAGEACTIONMENU = 59,
            OLECMDID_ADDTRAVELENTRY = 60,
            OLECMDID_UPDATETRAVELENTRY = 61,
            OLECMDID_UPDATEBACKFORWARDSTATE = 62,
            OLECMDID_OPTICAL_ZOOM = 63,
            OLECMDID_OPTICAL_GETZOOMRANGE = 64,
            OLECMDID_WINDOWSTATECHANGED = 65,
            OLECMDID_ACTIVEXINSTALLSCOPE = 66,
            OLECMDID_UPDATETRAVELENTRY_DATARECOVERY = 67,
            OLECMDID_SHOWTASKDLG = 68,
            OLECMDID_POPSTATEEVENT = 69,
            OLECMDID_VIEWPORT_MODE = 70,
            OLECMDID_LAYOUT_VIEWPORT_WIDTH = 71,
            OLECMDID_VISUAL_VIEWPORT_EXCLUDE_BOTTOM = 72,
            OLECMDID_USER_OPTICAL_ZOOM = 73,
            OLECMDID_PAGEAVAILABLE = 74,
            OLECMDID_GETUSERSCALABLE = 75,
            OLECMDID_UPDATE_CARET = 76,
            OLECMDID_ENABLE_VISIBILITY = 77,
            OLECMDID_MEDIA_PLAYBACK = 78,
            OLECMDID_SETFAVICON = 79,
            OLECMDID_SET_HOST_FULLSCREENMODE = 80,
            OLECMDID_EXITFULLSCREEN = 81,
            OLECMDID_SCROLLCOMPLETE = 82,
            OLECMDID_ONBEFOREUNLOAD = 83,
            OLECMDID_SHOWMESSAGE_BLOCKABLE = 84,
            OLECMDID_SHOWTASKDLG_BLOCKABLE = 85
        };
        enum OLECMDEXECOPT {
            OLECMDEXECOPT_DODEFAULT = 0,
            OLECMDEXECOPT_PROMPTUSER = 1,
            OLECMDEXECOPT_DONTPROMPTUSER = 2,
            OLECMDEXECOPT_SHOWHELP = 3
        };
        public static void SetZoom(this WebBrowser webBrowser, double Zoom) {
            // Get the _axIWebBrowser2
            var wb = (dynamic)webBrowser.GetType().GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(webBrowser);
            // For some reason the webbrower does update its scaling, but without drawing the page bigger.
            // Therefore the zoomLevel should be the scaling (in decimal) squared for correct updates.
            int zoomLevel = (int)(Zoom * Zoom * 100);
            // Use ExecWB command with OLECMDID_OPTICAL_ZOOM (63) and we do not want user promts OLECMDEXECOPT_DONTPROMPTUSER (2).
            // This is a internal IE function.
            wb.ExecWB((int)OLECMDID.OLECMDID_OPTICAL_ZOOM, (int)OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, zoomLevel, ref zoomLevel);

        }
    }
}
