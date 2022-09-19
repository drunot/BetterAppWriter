using BetterAW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TranslationCreator {
    internal class TranslationLine: Control {

        public string OriginalText {
            get { return (string)GetValue(OriginalTextProperty); }
            set { SetValue(OriginalTextProperty, value); }
        }

        public static readonly DependencyProperty OriginalTextProperty = DependencyProperty.Register("OriginalText", typeof(string), typeof(TranslationLine));
        public string TranslationText {
            get { return (string)GetValue(TranslationTextProperty); }
            set { SetValue(TranslationTextProperty, value); }
        }

        public static readonly DependencyProperty TranslationTextProperty = DependencyProperty.Register("TranslationText", typeof(string), typeof(TranslationLine));
    }
}
