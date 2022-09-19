using BetterAW;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TranslationCreator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            LoadTranslation(BetterAW.Translation.Instance);
        }

        public void LoadTranslation(BetterAW.Translation translation) {
            var origTrans = (BetterAW.Translation)typeof(BetterAW.Translation).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0].Invoke(new Object[] { });
            foreach (var member in typeof(BetterAW.Translation).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).PropertyType == typeof(string)) {
                    var line = new TranslationLine();
                    line.OriginalText = (member as PropertyInfo).GetValue(origTrans) as string;
                    line.TranslationText = (member as PropertyInfo).GetValue(translation) as string;
                    ContentPanel.Children.Insert(ContentPanel.Children.Count-1, line);

                }
            }
        }
        public void SetTranslation(BetterAW.Translation translation) {
            var counter = 0;
            foreach (var member in typeof(BetterAW.Translation).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).PropertyType == typeof(string)) {
                    ContentPanel.Children.OfType<TranslationLine>().ToArray()[counter++].TranslationText = (string)(member as PropertyInfo).GetValue(translation);
                }
            }
        }

        public BetterAW.Translation GetTranslation() {
            var instance = BetterAW.Translation.Instance;
            var counter = 0;
            foreach (var member in typeof(BetterAW.Translation).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).PropertyType == typeof(string)) {
                    (member as PropertyInfo).SetValue(instance, ContentPanel.Children.OfType<TranslationLine>().ToArray()[counter++].TranslationText);
                }
            }

            return instance;

        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = "lang";
            openFileDialog.Filter = "Language file (*.lang)|*.lang";
            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName)) {
                BetterAW.Translation.ReadFromBinaryFile(openFileDialog.FileName);
                SetTranslation(BetterAW.Translation.Instance);

            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "lang";
            saveFileDialog.Filter = "Language file (*.lang)|*.lang";
            if (saveFileDialog.ShowDialog() == true) {
                GetTranslation();
                BetterAW.Translation.Instance.WriteToBinaryFile(saveFileDialog.FileName);

            }
        }
    }
}
