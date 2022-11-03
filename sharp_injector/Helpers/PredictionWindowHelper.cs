using BetterAW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Media;
using BetterAW.Helpers;
using sharp_injector.Patches;
using HarmonyLib;

namespace sharp_injector.Helpers {
    internal static class PredictionWindowHelper {

        private enum PredictionType {
            PrimaryPredictions = 0,
            SecondaryPredictions
        }

        private static string GetPredictionStackPanelName(PredictionType pt) {
            switch (pt) {
                case PredictionType.PrimaryPredictions:
                    return "PrimaryPredictions";
                case PredictionType.SecondaryPredictions:
                    return "SecondaryPredictions";
                default:
                    return "";
            }
        }





        private static PredictionType curPredictionType = PredictionType.PrimaryPredictions;

        public static void IncrementSelection(Window predictionWindow) {
            predictionWindow.Dispatcher.Invoke(new Action(() => {
                try {
                    //_selectedPrediction
                    var pwType = predictionWindow.GetType();
                    var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
                    if (selectedPrediction is null || !DictionaryPredictionsVisible(predictionWindow)) {
                        curPredictionType = PredictionType.PrimaryPredictions;
                    }
                    var SpeechModelPredictions = (StackPanel)pwType.GetField(GetPredictionStackPanelName(curPredictionType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    int next = 0;
                    if (!(selectedPrediction is null)) {
                        var temp = new List<Decorator>(SpeechModelPredictions.Children.OfType<Decorator>());
                        next = temp.FindIndex(x => (DockPanel)x.Child == selectedPrediction) + 1;
                        if (next >= temp.Where(x => x.Visibility == Visibility.Visible).Count()) {
                            next = 0;
                            IncrementPage(predictionWindow);
                        }
                        //var predictionType = selectedPrediction.Tag == "SpeechModelPrediction" ? 0 : 1;
                        var curSelected = pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                        pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { curSelected });
                    }


                    pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child);
                    pwType.GetMethod("HighlightPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child });

                    // Read preidction if enabled
                    if (ReadPredictions) {
                        pwType.GetMethod("StopReading", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, null);
                        pwType.GetMethod("ReadPredictionTimeout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child });
                    }

                    // Set NavigatingPredictions to true
                    var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    var _serviceType = _service.GetType();
                    _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, true);
                } catch (Exception e) {
                    Terminal.Print($"{e}\n");
                }
            }));
        }

        public static void DecrementSelection(Window predictionWindow) {
            predictionWindow.Dispatcher.Invoke(new Action(() => {
                try {
                    //_selectedPrediction
                    var pwType = predictionWindow.GetType();
                    var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
                    if (selectedPrediction is null || !DictionaryPredictionsVisible(predictionWindow)) {
                        curPredictionType = PredictionType.PrimaryPredictions;
                    }
                    var SpeechModelPredictions = (StackPanel)pwType.GetField(GetPredictionStackPanelName(curPredictionType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    var temp = new List<Decorator>(SpeechModelPredictions.Children.OfType<Decorator>());
                    int next = temp.Where(x => x.Visibility == Visibility.Visible).Count() - 1;
                    if (!(selectedPrediction is null)) {
                        next = temp.FindIndex(x => (DockPanel)x.Child == selectedPrediction) - 1;
                        if (next < 0) {
                            next = temp.Where(x => x.Visibility == Visibility.Visible).Count() - 1;
                            DecrementPage(predictionWindow);
                        }
                        //var predictionType = selectedPrediction.Tag == "SpeechModelPrediction" ? 0 : 1;
                        var curSelected = pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                        pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { curSelected });


                    }

                    pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child);
                    pwType.GetMethod("HighlightPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child });

                    // Read preidction if enabled
                    if (ReadPredictions) {
                        pwType.GetMethod("StopReading", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, null);
                        pwType.GetMethod("ReadPredictionTimeout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[next]).Child });
                    }
                    // Set NavigatingPredictions to true
                    var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    var _serviceType = _service.GetType();
                    _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, true);

                } catch (Exception e) {
                    Terminal.Print($"{e}\n");
                }
            }));
        }

        public static bool IncrementPage(Window predictionWindow, bool wrap = false) {
            if (!DictionaryPredictionsVisible(predictionWindow)) {
                curPredictionType = PredictionType.PrimaryPredictions;
            }
            var pwType = predictionWindow.GetType();
            var _currentPredictions = pwType.GetField("_currentPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _currentSpeechModelPage = (int)pwType.GetField("_currentPrimaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _currentDictionaryPage = (int)pwType.GetField("_currentSecondaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            // Set NavigatingPredictions to false
            var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _serviceType = _service.GetType();
            _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, false);
            if ((curPredictionType == PredictionType.PrimaryPredictions)) {
                var _currentSpeechModelMaxPages = (int)pwType.GetField("_currentPrimaryPredictionsMaxPages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                if (_currentSpeechModelPage >= _currentSpeechModelMaxPages) {
                    if (wrap) {
                        pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, 1, _currentDictionaryPage });
                    }
                    return false;
                }

                pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage + 1, _currentDictionaryPage });

                return true;
            }
            var _currentDictionaryMaxPages = (int)pwType.GetField("_currentSecondaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            if (_currentDictionaryPage >= _currentDictionaryMaxPages) {
                if (wrap) {
                    pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage, 1 });
                }

                return false;
            }

            pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage, _currentDictionaryPage + 1 });

            return true;
        }

        public static bool DecrementPage(Window predictionWindow, bool wrap = false) {
            if (!DictionaryPredictionsVisible(predictionWindow)) {
                curPredictionType = PredictionType.PrimaryPredictions;
            }
            var pwType = predictionWindow.GetType();
            var _currentPredictions = pwType.GetField("_currentPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _currentSpeechModelPage = (int)pwType.GetField("_currentPrimaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _currentDictionaryPage = (int)pwType.GetField("_currentSecondaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);

            // Set NavigatingPredictions to false
            var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _serviceType = _service.GetType();
            _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, false);
            if ((curPredictionType == PredictionType.PrimaryPredictions)) {
                if (_currentSpeechModelPage <= 1) {
                    if (wrap) {
                        var _currentSpeechModelMaxPages = (int)pwType.GetField("_currentPrimaryPredictionsMaxPages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                        pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelMaxPages, _currentDictionaryPage });
                    }

                    return false;
                }

                pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage - 1, _currentDictionaryPage });

                return true;
            }
            if (_currentDictionaryPage <= 1) {
                if (wrap) {
                    var _currentDictionaryMaxPages = (int)pwType.GetField("_currentSecondaryPredictionsPage", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage, _currentDictionaryMaxPages });
                }

                return false;
            }
            pwType.GetMethod("ShowPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { _currentPredictions, _currentSpeechModelPage, _currentDictionaryPage - 1 });

            return true;
        }

        public static void TogglePredictionType(Window predictionWindow) {

            var pwType = predictionWindow.GetType();
            var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
            if (selectedPrediction is null) {
                curPredictionType = curPredictionType == PredictionType.SecondaryPredictions ? PredictionType.PrimaryPredictions : PredictionType.SecondaryPredictions;
                return;
            }
            var curPredictions = (StackPanel)pwType.GetField(GetPredictionStackPanelName(curPredictionType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var curPredList = new List<Decorator>(curPredictions.Children.OfType<Decorator>());
            var curIdx = curPredList.FindIndex(x => (DockPanel)x.Child == selectedPrediction);
            curPredictionType = curPredictionType == PredictionType.SecondaryPredictions ? PredictionType.PrimaryPredictions : PredictionType.SecondaryPredictions;
            var nextPredictions = (StackPanel)pwType.GetField(GetPredictionStackPanelName(curPredictionType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var nextPredList = new List<Decorator>(nextPredictions.Children.OfType<Decorator>());

            // Assume next idx to be max.
            var nextIdx = nextPredList.Where(x => x.Visibility == Visibility.Visible).Count() - 1;

            // If curIdx is less than max, set to curIdx.
            if (curIdx < nextPredList.Where(x => x.Visibility == Visibility.Visible).Count()) {
                nextIdx = curIdx;
            }

            // Reset old selected.
            var curSelected = pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { curSelected });

            // Set new selection.
            pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, (DockPanel)((Decorator)nextPredictions.Children[nextIdx]).Child);
            pwType.GetMethod("HighlightPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)nextPredictions.Children[nextIdx]).Child });

            // Read preidction if enabled
            if (ReadPredictions) {
                pwType.GetMethod("StopReading", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, null);
                pwType.GetMethod("ReadPredictionTimeout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)nextPredictions.Children[nextIdx]).Child });
            }
            // Set NavigatingPredictions to true
            var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _serviceType = _service.GetType();
            _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, true);
        }

        public static bool DictionaryPredictionsVisible(Window predictionWindow) {
            var pwType = predictionWindow.GetType();
            var AppWriterService = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var AppWriterServiceType = AppWriterService.GetType();
            var Profile = AppWriterServiceType.GetProperty("Profile", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(AppWriterService);
            var ProfileType = Profile.GetType();

            return (bool)ProfileType.GetField("GlossaryPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Profile);
        }

        //
        public static bool ReadPredictions {
            get {
                var Configuration = Type.GetType("AppWriter.ConfigurationManager,AppWriter").GetProperty("Configuration", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(null);
                var ConfigurationType = Configuration.GetType();
                return (bool)ConfigurationType.GetProperty("ReadPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Configuration);
            }
        }
        public static int SecondaryPredictionsCount(Window predictionWindow) {

            var pwType = predictionWindow.GetType();
            return (int)pwType.GetField("_currentSecondaryPredictionsMaxPages", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
        }
        public static void TogglePredictioTypeOrIncrement(Window predictionWindow) {
            predictionWindow.Dispatcher.Invoke(new Action(() => {
                if (DictionaryPredictionsVisible(predictionWindow) && SecondaryPredictionsCount(predictionWindow) > 0) {
                    TogglePredictionType(predictionWindow);
                } else {
                    IncrementPage(predictionWindow, true);
                }
            }));
        }
        public static void TogglePredictioTypeOrDecrement(Window predictionWindow) {
            predictionWindow.Dispatcher.Invoke(new Action(() => {
                if (DictionaryPredictionsVisible(predictionWindow) && SecondaryPredictionsCount(predictionWindow) > 0) {
                    TogglePredictionType(predictionWindow);
                } else {
                    DecrementPage(predictionWindow, true);
                }
            }));
        }

        public static bool InsertSelectedPrediction(Window predictionWindow) {
            var pwType = predictionWindow.GetType();
            var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
            if (selectedPrediction is null) {
                return false;
            }
            predictionWindow.Dispatcher.Invoke(new Action(() => {
                var index = (int)pwType.GetMethod("GetPredictionIndex", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { selectedPrediction });
                pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { selectedPrediction });
                pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, null);
                if (index == -1)
                    return;
                //_service
                var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                var _serviceType = _service.GetType();
                var _currentPredictions = pwType.GetField("_currentPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                var currentPredictionsType = _currentPredictions.GetType();
                var PredictionResult = currentPredictionsType.GetProperty("PredictionResult", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(_currentPredictions);
                _serviceType.GetMethod("InsertPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(_service, new object[] { PredictionResult, curPredictionType, index });
                predictionWindow.Visibility = Visibility.Collapsed;

                // Set NavigatingPredictions to false
                _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, false);
            }));
            return true;
        }


        public static void SelectPredictionIndex(Window predictionWindow, int index) {
            if (predictionWindow is null || !predictionWindow.IsVisible) {
                return;
            }
            curPredictionType = PredictionType.PrimaryPredictions;
            var pwType = predictionWindow.GetType();
            var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
            if (!(selectedPrediction is null)) {
                pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { selectedPrediction });
            }
            var SpeechModelPredictions = (StackPanel)pwType.GetField(GetPredictionStackPanelName(curPredictionType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var temp = new List<Decorator>(SpeechModelPredictions.Children.OfType<Decorator>());
            pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, (DockPanel)((Decorator)SpeechModelPredictions.Children[index]).Child);
            pwType.GetMethod("HighlightPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[index]).Child });

            // Read preidction if enabled
            if (ReadPredictions) {
                pwType.GetMethod("StopReading", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, null);
                pwType.GetMethod("ReadPredictionTimeout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { (DockPanel)((Decorator)SpeechModelPredictions.Children[index]).Child });
            }

            // Set NavigatingPredictions to true
            var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
            var _serviceType = _service.GetType();
            _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, true);
        }

        public static void DeselectPrediction(Window predictionWindow) {
            curPredictionType = PredictionType.PrimaryPredictions;
            var pwType = predictionWindow.GetType();
            var selectedPrediction = ((System.Windows.Controls.DockPanel)pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow));
            if (!(selectedPrediction is null)) {
                pwType.GetMethod("RestorePredictionColor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(predictionWindow, new object[] { selectedPrediction });
                pwType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(predictionWindow, null);


                // Set NavigatingPredictions to false
                var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                var _serviceType = _service.GetType();
                _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, false);
            }

        }

        private delegate void UpdateShortcutTextDelegate(Window predictionWindow);
        private static Queue<UpdateShortcutTextDelegate> toUpdate = new Queue<UpdateShortcutTextDelegate>();

        private static void Postfix_Prediction_Window_Loaded(ref Window __instance) {
            while (toUpdate.Any()) {
                (toUpdate.Dequeue())(__instance);
            }

        }

        public static void UpdateShortcutTextOnLoad() {
            var pwType = Type.GetType("AppWriter.Windows.PredictionsWindow,AppWriter");
            var mPostfix_Prediction_Window_Loaded = typeof(PredictionWindowHelper).GetMethod(nameof(Postfix_Prediction_Window_Loaded), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var predictionWindow_Loaded = pwType.GetMethod("Window_Loaded", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            PatchRegister.HarmonyInstance.Patch(predictionWindow_Loaded, null, new HarmonyMethod(mPostfix_Prediction_Window_Loaded));
        }

        public static void UpdateShortcutText(Window predictionWindow, int idx, SortedSet<Keys> keyboardShortcut) {

            if (predictionWindow is null) {
                Terminal.Print("predictionWindow is null\n");
                // Add this with the ability to update the prediction window.
                toUpdate.Enqueue((pw) => UpdateShortcutText(pw, idx, keyboardShortcut));
            } else {
                predictionWindow.Dispatcher.Invoke(() => {
                    var pwType = predictionWindow.GetType();
                    var numbersSP = (StackPanel)pwType.GetField("PredictionNumbers", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow);
                    var border = (Border)numbersSP.Children[idx];
                    if (keyboardShortcut is null) {
                        border.ToolTip = null;
                        ((TextBlock)border.Child).Text = "";
                        return;
                    }


                    string msgString = Helpers.Translations.GetString("Press_alt_number_to_insert_word_prediction");

                    // Remove Alt+
                    msgString = Regex.Replace(msgString, @"(?:[^ ]+)(?=\{0\})", "");

                    var shortcutString = keyboardShortcut.CustomToString();
                    msgString = String.Format(msgString, shortcutString);
                    var lastShortcut = keyboardShortcut.Last().CustomToString();


                    border.ToolTip = msgString;
                    ((TextBlock)border.Child).Text = lastShortcut;
                });
            }
        }

    }
}
