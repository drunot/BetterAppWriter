using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BetterAW {
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class Terminal : Window {
        public static Terminal Instance { get => t_; }
        private Queue<string> printQueue_ = new Queue<string>();
        private static Terminal t_ = null;
        public static bool IsReady { get; private set; } = false;
        public static void Initialize() {
            if (Terminal.IsReady) {
                return;
            }
            t_ = new Terminal();
            IsReady = true;
            var timer1 = new Timer();
            timer1.Interval = 100;
            timer1.Elapsed += new ElapsedEventHandler(handleQueue);
            timer1.Start();
        }
        public static void Print(string msg) {
            if (!Terminal.IsReady) {
                return;
            }
            t_._Print(msg);
        }
        private static void handleQueue(object sender, EventArgs e) {
            string t = string.Empty;
            while (t_.printQueue_.Count != 0) {
                t += t_.printQueue_.Dequeue();
            }
            if (t != string.Empty) {
                t_.TextBlockTerminal.Dispatcher.Invoke(new Action(() => {
                    t_.TextBlockTerminal.SelectionStart = t_.TextBlockTerminal.Text.Length;
                    t_.TextBlockTerminal.SelectionLength = 0;
                    t_.TextBlockTerminal.Text += t; t_.ScrollViewerTerminal.ScrollToEnd();
                }));
            }

        }
        public static void Clear() {
            if (!Terminal.IsReady) {
                return;
            }
            t_._Clear();
        }
        public static void Toggle() {
            if (!IsReady) {
                return;
            }
            if (t_.Visibility == Visibility.Visible) {
                t_.Dispatcher.Invoke(new Action(() => t_.Visibility = Visibility.Collapsed));
            } else {
                t_.Dispatcher.Invoke(new Action(() => t_.Visibility = Visibility.Visible));
            }
        }
        private Terminal() {
            InitializeComponent();
        }

        private delegate void HandleString();


        private void _Print(string toPrint) {
            printQueue_.Enqueue(toPrint);
        }
        private void _Clear() {
            TextBlockTerminal.Dispatcher.Invoke(DispatcherPriority.Normal, new HandleString(() => { TextBlockTerminal.Text = ""; ScrollViewerTerminal.ScrollToVerticalOffset(0); }));

        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }

        private void Button_SearchClose_Click(object sender, RoutedEventArgs e) {
            Grid_SearchGrid.Visibility = Visibility.Collapsed;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                if (Keyboard.IsKeyDown(Key.F)) {
                    Grid_SearchGrid.Visibility = Grid_SearchGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                    TextBlock_Search.Focus();
                }
            }
        }

        static int SearchIdx = 0;
        private void Terminal_Search(string text, int idx = 0) {
            var textIdx = TextBlockTerminal.Text.ToLower().IndexOf(text.ToLower());
            for (int i = 0; i < idx; i++) {
                textIdx += text.Length;
                textIdx = TextBlockTerminal.Text.ToLower().IndexOf(text.ToLower(), textIdx);
                if (textIdx < 0) {
                    textIdx = TextBlockTerminal.Text.ToLower().IndexOf(text.ToLower());
                    SearchIdx = 0;
                    break;
                }
            }
            if (textIdx > -1) {
                TextBlockTerminal.Select(textIdx, text.Length);
                ScrollViewerTerminal.ScrollToVerticalOffset(TextBlockTerminal.GetRectFromCharacterIndex(textIdx).Top);
            }
        }
        private void TextBlock_Search_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex r = new Regex(@"[\r\n]+");
            if (r.IsMatch(e.Text)) {
                ++SearchIdx;
                Terminal_Search(TextBlock_Search.Text.Trim(), SearchIdx);
                e.Handled = false;
            } else {
                SearchIdx = 0;
            }
        }

        private void TextBlock_Search_TextChanged(object sender, TextChangedEventArgs e) {
            if (Keyboard.IsKeyDown(Key.Back)) {
                SearchIdx = 0;
            }
            Terminal_Search(TextBlock_Search.Text.Trim(), SearchIdx);
        }

        private void TextBlock_Search_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Back) {
                SearchIdx = 0;
            }
        }
    }
}
