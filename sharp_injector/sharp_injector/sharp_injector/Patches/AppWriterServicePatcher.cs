using BetterAW;
using HarmonyLib;
using sharp_injector.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace sharp_injector.Patches
{
    internal class AppWriterServicePatcher : IPatcher
    {
        static object toolbarWindowWindow_;
        static object appWriterService_;
        static Type appWriterServiceType_;
        public AppWriterServicePatcher(object toolbarWindowWindow)
        {
            PatchRegister.RegisterPatch(this);
            toolbarWindowWindow_ = toolbarWindowWindow;
            appWriterServiceType_ = Type.GetType("AppWriter.AppWriterService,AppWriter.Core");
            //ClassPrinter.PrintMembers("AppWriter.Xaml.Elements.Translation.Translations,AppWriter.Core");
            appWriterService_ = toolbarWindowWindow_.GetType().GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindowWindow_);
            
        }

        static void LanguagePatch(string currentLanguges)
        {
            // Add en-US and en-GB to GeneralDictionaryLanguages in order to show the dictonary button for these as well.
            var Globals = Type.GetType("AppWriter.Globals,AppWriter.Core");
            var GeneralDictionaryLanguages = (ReadOnlyCollection<string>)Globals.GetField("GeneralDictionaryLanguages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(null);
            List<string> GeneralDictionaryLanguagesList = new List<string>();
            foreach (string language in GeneralDictionaryLanguages)
            {
                GeneralDictionaryLanguagesList.Add(language);
            }
            GeneralDictionaryLanguagesList.Add("en-US");
            GeneralDictionaryLanguagesList.Add("en-GB");
            GeneralDictionaryLanguages = new ReadOnlyCollection<string>(GeneralDictionaryLanguagesList);
            Globals.GetField("GeneralDictionaryLanguages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(null, new ReadOnlyCollection<string>(GeneralDictionaryLanguagesList));

            // Because the window is already created at this point, manually add the button back for the languages en-US and en-GB.
            switch (currentLanguges)
            {
                case "en-GB":
                case "en-US":
                    ((Window)toolbarWindowWindow_).Dispatcher.Invoke(new Action(() =>
                    {
                        var DictionaryLookUpBtnInfo = toolbarWindowWindow_.GetType().GetField("DictionaryLookUpBtn", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                        Control DictionaryLookUpBtn = DictionaryLookUpBtnInfo.GetValue(toolbarWindowWindow_) as Control;
                        DictionaryLookUpBtn.Visibility = Visibility.Visible;

                    }));
                    break;
                default:
                    break;
            }
        }

        public static List<string> GetAvalibleLanguage()
        {
            try
            {
                object UserInfo = appWriterServiceType_.GetProperty("UserInfo", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(appWriterService_, null);
                object AWLicenseInfo = Type.GetType("AppWriter.LingApps_API.UserInfo,AppWriter.Core").GetProperty("AWLicenseInfo", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(UserInfo, null);
                object Settings = Type.GetType("AppWriter.LingApps_API.LicenseInfo,AppWriter.Core").GetField("Settings", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(AWLicenseInfo);
                return (List<string>)Type.GetType("AppWriter.LingApps_API.LicenseSettings,AppWriter.Core").GetField("Languages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Settings);
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
            return null;
        }
        public void Patch()
        {
            try
            {
                ChangeProfile_Patch();
                var lang = GetCurrentLanguage();
                Terminal.Print($"Language on startup {lang}\n");
                LanguagePatch(lang);
                PatchDictonaryButton();
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public static string GetCurrentLanguage()
        {
            
            var ProfileType = Type.GetType("AppWriter.AppWriterService,AppWriter.Core").GetProperty("Profile", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.CreateInstance);
            object Profile = ProfileType.GetValue(appWriterService_, null);
            var LingApps_API_Profile = Type.GetType("AppWriter.LingApps_API.Profile,AppWriter.Core");
            return LingApps_API_Profile.GetField("WordPredictionsLanguage").GetValue(Profile) as string;
        }
        /* Change Profile Patch Elements */
        static void ChangeProfile_Patch()
        {
            var mChangeProfile_Prefix = typeof(AppWriterServicePatcher).GetMethod(nameof(ChangeProfile_Prefix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var mChangeProfile_Postfix = typeof(AppWriterServicePatcher).GetMethod(nameof(ChangeProfile_Postfix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            var ChangeProfile = appWriterServiceType_.GetMethod("ChangeProfile", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            PatchRegister.HarmonyInstance.Patch(ChangeProfile, new HarmonyMethod(mChangeProfile_Prefix), new HarmonyMethod(mChangeProfile_Postfix));
        }

        static void ChangeProfile_Prefix(out string __state, string profileName)
        {
            __state = profileName;
        }

        static void ChangeProfile_Postfix(string __state)
        {
            try
            {
                Terminal.Print($"Language changed to {__state}\n");
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        /* Dictronary Button Patch */
        static MethodInfo mDictionaryLookup;
        static void PatchDictonaryButton()
        {
            // Find the correct Dictionary Lookup
            foreach(var member in appWriterServiceType_.GetMember("DictionaryLookup", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.CreateInstance))
            {
                if(member.MemberType != MemberTypes.Method || ((MethodInfo)member).GetParameters().Count() != 2 || ((MethodInfo)member).GetParameters()[0].ParameterType != typeof(string) || ((MethodInfo)member).GetParameters()[1].ParameterType != typeof(bool))
                {
                    continue;
                }
                mDictionaryLookup = (MethodInfo)member;
                break;
            }
            Type[] nestType = Type.GetType("AppWriter.Windows.ToolbarWindow,AppWriter").GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            // Find all nested types and patch the hell out of them if they contain DictionaryLookup!
            var mChangeProfile_Transpiler = typeof(AppWriterServicePatcher).GetMethod(nameof(ChangeProfile_Transpiler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (Type t in nestType)
            {
                foreach(var method in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (method.IsVirtual || method.IsConstructor || method.DeclaringType != t)
                    {
                        continue;
                    }
                    PatchRegister.HarmonyInstance.Patch(method, null, null, new HarmonyMethod(mChangeProfile_Transpiler));
                }
            }
        }
        static IEnumerable<CodeInstruction> ChangeProfile_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (((MethodInfo)codes[i].operand).Name == "DictionaryLookup")
                    {

                        // Crude way to find reference to caller:
                        int d = 1;
                        while(i > d)
                        {
                            if(codes[i - d].operand == null || codes[i - d].opcode != OpCodes.Ldfld)
                            {
                                d++;
                                continue;
                            }
                            if (((FieldInfo)codes[i - d].operand).Name == "_service")
                            {
                                //Remove reference to caller since we'll do static call
                                codes[i - (d + 2)].opcode = OpCodes.Nop;
                                codes[i - (d + 2)].operand = null;
                                codes[i - (d + 1)].opcode = OpCodes.Nop;
                                codes[i - (d + 1)].operand = null;
                                codes[i - d].opcode = OpCodes.Nop;
                                codes[i - d].operand = null;
                                break;
                            }
                            d++;
                        }
                        // Add the static call
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = typeof(AppWriterServicePatcher).GetMethod(nameof(DictionaryLookup), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    }
                }
            }

            return codes.AsEnumerable();
        }

        static Uri DictionaryLookup(string selectedText, bool isOnlineUrl)
        {
            switch(GetCurrentLanguage())
            {
                case "en-GB":
                case "en-US":
                    return new Uri("https://www.oxfordlearnersdictionaries.com/definition/english/" + selectedText.ToLower());
                default:
                    try
                    {
                       
                        return mDictionaryLookup.Invoke(appWriterService_, new object[] { selectedText, isOnlineUrl }) as Uri;
                    }
                    catch (Exception ex)
                    {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        return new Uri("https://google.com");
                    }
            }
        }
    }
}
