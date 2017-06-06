using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace QiitaStocksViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;
            this.Height = settings.Window_Heght;
            this.Width = settings.Window_Width;
            this.Top = settings.Window_Top;
            this.Left = settings.Window_Left;
            this.WindowState = (WindowState)settings.Window_State;
            this.CheckBox_isSaveUserID.IsChecked = settings.isSaveUserID;
            this.CheckBox_isSaveAccessToken.IsChecked = settings.isSaveAccessToken;
            this.TextBox_UserID.Text = settings.UserID;
            this.TextBox_AccessToken.Text = settings.AccessToken;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = Properties.Settings.Default;
            settings.Window_Heght = this.Height;
            settings.Window_Width = this.Width;
            settings.Window_Top = this.Top;
            settings.Window_Left = this.Left;
            if (this.WindowState != WindowState.Minimized)
            {
                settings.Window_State = (int)this.WindowState;
            }
            else
            {
                settings.Window_State = 0;
            }
            settings.isSaveUserID = this.CheckBox_isSaveUserID.IsChecked.Value;
            if (this.CheckBox_isSaveUserID.IsChecked.Value)
            {
                settings.UserID = this.TextBox_UserID.Text;
            }
            else
            {
                settings.UserID = "";
            }
            settings.isSaveAccessToken = this.CheckBox_isSaveAccessToken.IsChecked.Value;
            if(this.CheckBox_isSaveAccessToken.IsChecked.Value)
            {
                settings.AccessToken = this.TextBox_AccessToken.Text;
            }
            else
            {
                settings.AccessToken = "";
            }
            settings.Save();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink link = (Hyperlink)e.OriginalSource;
                Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch (Exception) { }
        }
    }
}
