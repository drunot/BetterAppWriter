using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace BetterAW
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class Terminal : Window
    {
        public static Terminal Instance { get => t_; }
        private Queue<string> printQueue_ = new Queue<string>();
        private static Terminal t_ = null;
        public static bool IsReady { get; private set; } = false;
        public static void Initialize()
        {
            if (Terminal.IsReady)
            {
                return;
            }
            t_ = new Terminal();
            IsReady = true;
            var timer1 = new Timer();
            timer1.Interval = 100;
            timer1.Elapsed += new ElapsedEventHandler(handleQueue);
            timer1.Start();
        }
        public static void Print(string msg)
        {
            if (!Terminal.IsReady)
            {
                return;
            }
            t_._Print(msg);
        }
        private static void handleQueue(object sender, EventArgs e)
        {
            string t = string.Empty;
            while(t_.printQueue_.Count != 0)
            {
                t += t_.printQueue_.Dequeue();
            }
            if(t != string.Empty)
            {
                t_.TextBlockTerminal.Dispatcher.Invoke(new Action(() => {
                    t_.TextBlockTerminal.SelectionStart = t_.TextBlockTerminal.Text.Length;
                    t_.TextBlockTerminal.SelectionLength = 0;
                    t_.TextBlockTerminal.Text += t; t_.ScrollViewerTerminal.ScrollToEnd(); }));
            }

        }
        public static void Clear()
        {
            if (!Terminal.IsReady)
            {
                return;
            }
            t_._Clear();
        }
        public static void Toggle()
        {
            if (!IsReady)
            {
                return;
            }
            if (t_.Visibility == Visibility.Visible)
            {
                t_.Dispatcher.Invoke(new Action(() => t_.Visibility = Visibility.Collapsed));
            } else
            {
                t_.Dispatcher.Invoke(new Action(() => t_.Visibility = Visibility.Visible));
            }
        }
        private Terminal()
        {
            InitializeComponent();
        }

        private delegate void HandleString();
        

        private void _Print(string toPrint)
        {
            printQueue_.Enqueue(toPrint);
        }
        private void _Clear()
        {
            TextBlockTerminal.Dispatcher.Invoke(DispatcherPriority.Normal, new HandleString(() => { TextBlockTerminal.Text = ""; ScrollViewerTerminal.ScrollToVerticalOffset(0); }));
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }
    }
}
