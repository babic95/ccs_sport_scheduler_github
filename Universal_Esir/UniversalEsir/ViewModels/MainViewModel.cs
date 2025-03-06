using UniversalEsir.Commands;
using UniversalEsir.State.Navigators;
using UniversalEsir.ViewModels.Activation;
using UniversalEsir.ViewModels.Login;
using UniversalEsir_Database;
using UniversalEsir_Database.Models;
using UniversalEsir_Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UniversalEsir.ViewModels
{
    public class MainViewModel : ViewModelBase, INavigator
    {
        private ViewModelBase _currentViewModel;

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        public MainViewModel(Window window)
        {
            Window = window;
            string pathToDB = SettingsManager.Instance.GetPathToDB();
            SqliteDbContext sqliteDbContext = new SqliteDbContext();
            sqliteDbContext.ConfigureDatabase(pathToDB);

            bool isActivationCodeNumberRequired = SettingsManager.Instance.IsActivationCodeNumberRequired();

            if (isActivationCodeNumberRequired)
            {
                CurrentViewModel = new ActivationViewModel(this);
            }
            else
            {
                if (SettingsManager.Instance.EnableSmartCard())
                {
                    CurrentViewModel = new LoginCardViewModel(UpdateCurrentViewModelCommand);
                }
                else
                {
                    CurrentViewModel = new LoginViewModel(UpdateCurrentViewModelCommand);
                }
            }

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = new Icon("icon.ico");
            _notifyIcon.Visible = true;
            CreateContextMenu();
        }

        public bool IsExit { get; private set; }
        public Window Window { get; private set; }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                OnPropertyChange(nameof(CurrentViewModel));
            }
        }
        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentAppStateViewModelCommand(this);
        public ICommand HiddenWindowCommand => new HiddenWindowCommand(Window);

        public CashierDB LoggedCashier { get; set; }

        #region Private Method
        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.Text = "CSS UniversalEsir";
            _notifyIcon.ContextMenuStrip.Items.Add("Otvori...").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Zatvori").Click += (s, e) => ExitApplication();
        }
        private void ExitApplication()
        {
            IsExit = true;
            Application.Current.Shutdown();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        private void ShowMainWindow()
        {
            if (Window.IsVisible)
            {
                if (Window.WindowState == WindowState.Minimized)
                {
                    Window.WindowState = WindowState.Normal;
                }
                Window.Activate();
            }
            else
            {
                Window.Show();
            }
        }
        #endregion Private Method
    }
}
