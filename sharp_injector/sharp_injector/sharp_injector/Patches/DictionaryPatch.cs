using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BetterAW;
using sharp_injector.Debug;
using System.Windows.Controls;
using DictionaryHandler;
using System.Windows;

namespace sharp_injector.Patches
{
    internal class DictionaryPatch : IPatcher
    {
        object dictionaryWindow_;
        WebBrowser Browser_;
        public DictionaryPatch(object dictionaryWindow)
        {
            try
            {
                // Get the controlls and register the patch
                dictionaryWindow_ = dictionaryWindow;
                Browser_ = (System.Windows.Controls.WebBrowser)dictionaryWindow_.GetType().GetField("Browser", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(dictionaryWindow_);
                ((Window)dictionaryWindow_).LocationChanged += (s, e) => SetScale();
                Browser_.LoadCompleted += (s, e) => SetScale();
                PatchRegister.RegisterPatch(this);
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public void Patch()
        {
            try {
                SurpressWarnings();
                HideInOxfordDom();
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        // Advanced learners dictionary create script errors. Ignore them!
        public static void HideScriptErrors(WebBrowser wb, bool hide)
        {
            try
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return;
                var objComWebBrowser = fiComWebBrowser.GetValue(wb);
                if (objComWebBrowser == null)
                {
                    wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                    return;
                }
                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        void SurpressWarnings()
        {
            Browser_.Dispatcher.Invoke(new Action(() => HideScriptErrors(Browser_, true)));
        }

        void HideInOxfordDom()
        {
            // Hide certen elements in the DOM for a better experience.
            // Todo: This is an event. Should be accesible from all threads. Test without dispatcher.
            Browser_.Dispatcher.Invoke(new Action(() => Browser_.LoadCompleted += (sender, e) => Terminal.Print(OxfordLearnersDictionaries.HideControls(Browser_))));
            
        }

        void SetScale()
        {
            // Make the scale consistent compaired to the Windows screen scale.
            try
            {
                PresentationSource source = PresentationSource.FromVisual((Window)dictionaryWindow_);
                 
                if (source != null)
                {
                    Browser_.SetZoom(source.CompositionTarget.TransformToDevice.M11);
                }
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
    }
}
