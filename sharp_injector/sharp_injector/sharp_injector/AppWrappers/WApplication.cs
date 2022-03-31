using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            FilePrinter.Print("Fuck1" + Environment.NewLine);
            var current = t_.GetProperty("Current").GetGetMethod(false).Invoke(null, null);
            FilePrinter.Print("Fuck2" + Environment.NewLine);
            Type ct = current.GetType();
            FilePrinter.Print("Fuck3" + Environment.NewLine);
            ct.GetMethod("Shutdown").Invoke(current, null);
            FilePrinter.Print("Fuck4" + Environment.NewLine);
        }

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            var current = t_.GetProperty("Current").GetGetMethod(false).Invoke(null, null);
            if(current == null)
            {
                FilePrinter.Print("current was null" + Environment.NewLine);
            }
            Type ct = current.GetType();
            var resources = ct.GetProperty("Resources").GetGetMethod(false).Invoke(current, null);
            if (resources == null)
            {
                FilePrinter.Print("resources was null" + Environment.NewLine);
            }
            Type rt = resources.GetType();
            var resourcesDir = (System.Collections.IDictionaryEnumerator)rt.GetProperty("GetEnumerator").GetGetMethod(false).Invoke(resources, null);
            return resourcesDir;
        }

    }
}
