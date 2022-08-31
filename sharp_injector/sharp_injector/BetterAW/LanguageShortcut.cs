using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterAW {
    public class LanguageShortcut : Control {
        // To Do: move EnabledLanguage responsibility away from this class
        public static HashSet<string> EnabledLanguage { get; private set; } = new HashSet<string>();
        public readonly string LanguageName;

        public delegate void ChnagedCheked(bool isChecked);
        public static ChnagedCheked ChnagedChekedEvent = (ChnagedCheked) => { };
        public LanguageShortcut() {
            LanguageName = string.Empty;
            SetValue(CheckedEventProperty, new RelayCommand(() => {
                // Add language to enabled languages.
                EnabledLanguage.Add(LanguageName);
                ChnagedChekedEvent(true);
            }));
            SetValue(UncheckedEventProperty, new RelayCommand(() => {
                // Remove language to enabled languages.
                EnabledLanguage.Remove(LanguageName);
                ChnagedChekedEvent(false);
            }));
        }
        public LanguageShortcut(string id) {
            try {
                LanguageName = id;
                SetValue(CheckedEventProperty, new RelayCommand(() => {
                    // Add language to enabled languages.
                    EnabledLanguage.Add(LanguageName);
                }));
                SetValue(UncheckedEventProperty, new RelayCommand(() => {
                    // Remove language to enabled languages.
                    EnabledLanguage.Remove(LanguageName);
                }));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public LanguageShortcut(string name, string shortcutText, bool selected, object icon = null) {
            try {
                LanguageName = name;
                if (icon != null) {
                    Icon = icon;
                    SetValue(IconVisibilityProperty, Visibility.Visible);

                }
                SetValue(ShortcutTextProperty, shortcutText);
                SetValue(CheckedEventProperty, new RelayCommand(() => {
                    // Add language to enabled languages.
                    EnabledLanguage.Add(LanguageName);
                    ChnagedChekedEvent(true);
                }));
                SetValue(UncheckedEventProperty, new RelayCommand(() => {
                    // Remove language to enabled languages.
                    EnabledLanguage.Remove(LanguageName);
                    ChnagedChekedEvent(false);
                }));
                SetValue(CheckedProperty, selected);
                if (selected) {
                    EnabledLanguage.Add(LanguageName);
                }
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public string ShortcutText {
            get { return (string)GetValue(ShortcutTextProperty); }
            set { SetValue(ShortcutTextProperty, value); }
        }

        public static readonly DependencyProperty ShortcutTextProperty = DependencyProperty.Register("ShortcutText", typeof(string), typeof(LanguageShortcut));

        public RelayCommand CheckedEvent {
            get { return (RelayCommand)GetValue(CheckedEventProperty); }
            set { SetValue(CheckedEventProperty, value); }
        }
        public static readonly DependencyProperty CheckedEventProperty = DependencyProperty.Register("CheckedEvent", typeof(RelayCommand), typeof(LanguageShortcut), new UIPropertyMetadata(new RelayCommand()));

        public RelayCommand UncheckedEvent {
            get { return (RelayCommand)GetValue(UncheckedEventProperty); }
            set { SetValue(UncheckedEventProperty, value); }
        }
        public static readonly DependencyProperty UncheckedEventProperty = DependencyProperty.Register("UncheckedEvent", typeof(RelayCommand), typeof(LanguageShortcut), new UIPropertyMetadata(new RelayCommand()));


        public bool? Checked {
            get { return (bool?)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool?), typeof(LanguageShortcut));

        public object Icon {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(LanguageShortcut));

        public Visibility IconVisibility {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            set { SetValue(IconVisibilityProperty, value); }
        }
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(LanguageShortcut), new UIPropertyMetadata(Visibility.Collapsed));
    }
}
