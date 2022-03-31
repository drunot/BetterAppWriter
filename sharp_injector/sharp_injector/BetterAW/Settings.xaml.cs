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
using System.Windows.Shapes;

namespace BetterAW
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public bool IsClosed = true;

        static KeyboardShortcuts keyboardShortcuts = null;
        public Settings()
        {
            InitializeComponent();
            this.IsVisibleChanged += (s, e) => { IsClosed = !this.IsVisible; };
            this.Closed += (s, e) => { IsClosed = true; };
        }

        private void TerminalBtn_Click(object sender, RoutedEventArgs e)
        {
            Terminal.Toggle();
        }

        public delegate void Test_Click(object sender, RoutedEventArgs e);

        public static Test_Click TestClick = (s, e) => { };

        private void KeyboardShortBtn_Click(object sender, RoutedEventArgs e)
        {
            if(keyboardShortcuts == null || keyboardShortcuts.IsClosed)
            {
                keyboardShortcuts = new KeyboardShortcuts();
                keyboardShortcuts.Show();
            } else
            {
                keyboardShortcuts.Focus();
            }
        }
    }
}
