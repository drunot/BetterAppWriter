using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterAW;
using sharp_injector.Debug;

namespace sharp_injector.AppWrappers
{
    class WApplication
    {
        private Type t_;
        public WApplication()
        {
            t_ = Type.GetType("System.Windows.Application,PresentationFramework");
            if(t_ == null)
            {
                throw new Exception("System.Windows.Application,PresentationFramework was not found");
            }
        }
        public void Shutdown()
        {
            var current = t_.GetProperty("Current").GetGetMethod(false).Invoke(null, null);
            Type ct = current.GetType();
            ct.GetMethod("Shutdown").Invoke(current, null);
        }

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            var current = t_.GetProperty("Current").GetGetMethod(false).Invoke(null, null);
            if(current == null)
            {
                Terminal.Print("current was null" + Environment.NewLine);
            }
            Type ct = current.GetType();
            var resources = ct.GetProperty("Resources").GetGetMethod(false).Invoke(current, null);
            if (resources == null)
            {
                Terminal.Print("resources was null" + Environment.NewLine);
            }
            Type rt = resources.GetType();
            var resourcesDir = (System.Collections.IDictionaryEnumerator)rt.GetProperty("GetEnumerator").GetGetMethod(false).Invoke(resources, null);
            return resourcesDir;
        }

    }
}
