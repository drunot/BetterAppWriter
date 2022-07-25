﻿using BetterAW.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterAW {
    public class KeyboardShortcut : Control {
        public delegate void KeyboardShortcutEvent(KeyboardShortcut self);
        public static KeyboardShortcutEvent AddEvent = (self) => { };
        public static KeyboardShortcutEvent RemoveEvent = (self) => { };

        private KeyboardShortcutInfo _shortcutInfo;

        public KeyboardShortcutInfo ShortcutInfo {
            get => _shortcutInfo;
            set {
                _shortcutInfo = value;
                SetValue(ShortcutKeybindTextProperty, _shortcutInfo.keyBinding.CustomToString());
            }
        }
        static KeyboardShortcut() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyboardShortcut), new FrameworkPropertyMetadata(typeof(KeyboardShortcut)));
        }

        public KeyboardShortcut() {
            try {
                SetValue(AddShorcutEventProperty, new RelayCommand(() => AddEvent(this)));
                SetValue(RemoveShorcutEventProperty, new RelayCommand(() => RemoveEvent(this)));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public KeyboardShortcut(KeyboardShortcutInfo keyboardShortcut) {
            try {
                _shortcutInfo = keyboardShortcut;
                SetValue(ShortcutTextProperty, _shortcutInfo.ShortcutText);
                SetValue(ShortcutKeybindTextProperty, _shortcutInfo.keyBinding.CustomToString());
                SetValue(AddShorcutEventProperty, new RelayCommand(() => AddEvent(this)));
                SetValue(RemoveShorcutEventProperty, new RelayCommand(() => RemoveEvent(this)));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public string ShortcutText {
            get { return (string)GetValue(ShortcutTextProperty); }
            set { SetValue(ShortcutTextProperty, value); }
        }

        public static readonly DependencyProperty ShortcutTextProperty = DependencyProperty.Register("ShortcutText", typeof(string), typeof(KeyboardShortcut));

        public string ShortcutKeybindText {
            get { return (string)GetValue(ShortcutKeybindTextProperty); }
            set { SetValue(ShortcutKeybindTextProperty, value); }
        }

        public static readonly DependencyProperty ShortcutKeybindTextProperty = DependencyProperty.Register("ShortcutKeybindText", typeof(string), typeof(KeyboardShortcut));

        public RelayCommand AddShorcutEvent {
            get { return (RelayCommand)GetValue(AddShorcutEventProperty); }
            set { SetValue(AddShorcutEventProperty, value); }
        }
        public static readonly DependencyProperty AddShorcutEventProperty = DependencyProperty.Register("AddShorcutEvent", typeof(RelayCommand), typeof(KeyboardShortcut), new UIPropertyMetadata(new RelayCommand()));

        public RelayCommand RemoveShorcutEvent {
            get { return (RelayCommand)GetValue(RemoveShorcutEventProperty); }
            set { SetValue(RemoveShorcutEventProperty, value); }
        }
        public static readonly DependencyProperty RemoveShorcutEventProperty = DependencyProperty.Register("RemoveShorcutEvent", typeof(RelayCommand), typeof(KeyboardShortcut), new UIPropertyMetadata(new RelayCommand()));
    }
}
