using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterAW
{
    public class LanguageShortcut : Control
    {
        // To Do: move EnabledLanguage responsibility away from this class
        public static HashSet<string> EnabledLanguage { get; private set; } = new HashSet<string>();
        private readonly string _id;
        
        public delegate void ChnagedCheked(bool isChecked);
        public static ChnagedCheked ChnagedChekedEvent = (ChnagedCheked) => { };
        public LanguageShortcut()
        {
            _id = string.Empty;
            SetValue(CheckedEventProperty, new RelayCommand(() => {
                Terminal.Print($"Checked: {_id}\n");
            }));
            SetValue(UncheckedEventProperty, new RelayCommand(() => {
                Terminal.Print($"Unchecked: {_id}\n");
            }));
        }
        public LanguageShortcut(string id)
        {
            try
            {
                _id = id;
                SetValue(CheckedEventProperty, new RelayCommand(() => {
                    Terminal.Print($"Checked: {_id}\n");
                    EnabledLanguage.Add(_id);
                }));
                SetValue(UncheckedEventProperty, new RelayCommand(() => {
                    Terminal.Print($"Unchecked: {_id}\n");
                    EnabledLanguage.Remove(_id);
                }));
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public LanguageShortcut(LanguageShortcutInfo info)
        {
            try
            {
                _id = info.Name;
                if(info.Icon != null)
                {
                    Icon = info.Icon;
                    SetValue(IconVisibilityProperty, Visibility.Visible);

                }
                SetValue(ShortcutTextProperty, info.ShortcutText);
                SetValue(CheckedEventProperty, new RelayCommand(() => {
                    Terminal.Print($"Checked: {_id}\n");
                    EnabledLanguage.Add(_id);
                    ChnagedChekedEvent(true);
                    }));
                SetValue(UncheckedEventProperty, new RelayCommand(() => {
                    Terminal.Print($"Unchecked: {_id}\n");
                    EnabledLanguage.Remove(_id);
                    ChnagedChekedEvent(false);
                }));
                SetValue(CheckedProperty, info.Selected);
                if (info.Selected)
                {
                    EnabledLanguage.Add(_id);
                }
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public string ShortcutText
        {
            get { return (string)GetValue(ShortcutTextProperty); }
            set { SetValue(ShortcutTextProperty, value); }
        }

        public static readonly DependencyProperty ShortcutTextProperty = DependencyProperty.Register("ShortcutText", typeof(string), typeof(LanguageShortcut));

        public RelayCommand CheckedEvent
        {
            get { return (RelayCommand)GetValue(CheckedEventProperty); }
            set { SetValue(CheckedEventProperty, value); }
        }
        public static readonly DependencyProperty CheckedEventProperty = DependencyProperty.Register("CheckedEvent", typeof(RelayCommand), typeof(LanguageShortcut), new UIPropertyMetadata(new RelayCommand()));

        public RelayCommand UncheckedEvent
        {
            get { return (RelayCommand)GetValue(UncheckedEventProperty); }
            set { SetValue(UncheckedEventProperty, value); }
        }
        public static readonly DependencyProperty UncheckedEventProperty = DependencyProperty.Register("UncheckedEvent", typeof(RelayCommand), typeof(LanguageShortcut), new UIPropertyMetadata(new RelayCommand()));


        public bool? Checked
        {
            get { return (bool?)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool?), typeof(LanguageShortcut));

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(LanguageShortcut));

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }
            set { SetValue(IconVisibilityProperty, value); }
        }
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(LanguageShortcut), new UIPropertyMetadata(Visibility.Collapsed));
    }
}
