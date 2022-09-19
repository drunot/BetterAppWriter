using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterAW {
    [ContentProperty("Content")]
    public class BaseWindowControl : Control {
        static BaseWindowControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindowControl), new FrameworkPropertyMetadata(typeof(BaseWindowControl)));
        }

        public BaseWindowControl() {
            SetValue(MoveEventProperty, new RelayCommand(() => this.DragMove()));
        }

        public object Content {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(BaseWindowControl));

        public RelayCommand MoveEvent {
            get { return (RelayCommand)GetValue(MoveEventProperty); }
            set { SetValue(MoveEventProperty, value); }
        }
        public static readonly DependencyProperty MoveEventProperty = DependencyProperty.Register("MoveEvent", typeof(RelayCommand), typeof(BaseWindowControl), new UIPropertyMetadata());

        private void DragMove() {
            try {
                Window.GetWindow(this).DragMove();
            } catch (InvalidOperationException e) {

            }
        }

        public String Title {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BaseWindowControl), new PropertyMetadata("Title"));
    }
}
