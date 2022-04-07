using BetterAW;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharp_injector.DTO
{
    public class JSONLanguage
    {
        public string Name;
        public bool Selected;

        public static JSONLanguage FromLangugeShortcutInfo(LanguageShortcutInfo languageShortcutInfo)
        {
            JSONLanguage ret = new JSONLanguage();
            ret.Name = languageShortcutInfo.Name;
            ret.Selected = languageShortcutInfo.Selected;
            return ret;
        }

        public static JSONLanguage FromJObject(JObject JObj)
        {
            JSONLanguage ret = new JSONLanguage();
            ret.Name = (string)JObj["Name"];
            ret.Selected = (bool)JObj["Selected"];
            return ret;
        }

        public LanguageShortcutInfo ToKeyboardShortcutInfo()
        {
            LanguageShortcutInfo info = new LanguageShortcutInfo();
            info.Name = Name;
            return info;
        }
    }
}
