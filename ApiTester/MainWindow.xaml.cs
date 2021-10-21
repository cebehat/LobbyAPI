using System;
using System.Collections.Generic;
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

namespace ApiTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RequestHandler requestHandler;
        public MainWindow()
        {
            InitializeComponent();
            requestHandler = new RequestHandler("https://lobbyrestapi20211016183500.azurewebsites.net/");
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            requestHandler.Create();
            RefreshLobbyList();
        }

        private void RefreshLobbyList()
        {
            LobbyListBox.Items.Clear();
            var lobbies = requestHandler.GetLobbies().ToList();
            lobbies.ForEach(l => LobbyListBox.Items.Add(l));
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLobbyList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(LobbyListBox.SelectedItem != null)
            {
                LobbyModel lobbyModel = (LobbyModel)LobbyListBox.SelectedItem;
                requestHandler.Delete(lobbyModel.LobbyId);
                RefreshLobbyList();
            }
        }

        private void LobbyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ShowIPButton_Click(object sender, RoutedEventArgs e)
        {
            if (LobbyListBox.SelectedItem == null) return;

            MessageBox.Show(requestHandler.GetHost(((LobbyModel)LobbyListBox.SelectedItem).LobbyId));
        }
    }
}
