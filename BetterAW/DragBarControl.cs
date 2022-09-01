using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public partial class DragBarControl : Control {
        static DragBarControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragBarControl), new FrameworkPropertyMetadata(typeof(DragBarControl)));
        }

        public DragBarControl() {
            SetValue(CloseClickEventProperty, new RelayCommand(() => this.Close()));
        }

        public String Title {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DragBarControl));

        // 
        public RelayCommand CloseClickEvent {
            get { return (RelayCommand)GetValue(CloseClickEventProperty); }
            set { SetValue(CloseClickEventProperty, value); }
        }
        public static readonly DependencyProperty CloseClickEventProperty = DependencyProperty.Register("CloseClickEvent", typeof(RelayCommand), typeof(DragBarControl), new UIPropertyMetadata(new RelayCommand()));

        public RelayCommand OtherCloseClickEvent = new RelayCommand();

        private void Close() {
            Window.GetWindow(this).Close();
        }
    }
}
