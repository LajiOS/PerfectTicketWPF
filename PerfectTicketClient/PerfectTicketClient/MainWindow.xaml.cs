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

using PerfectTicketClient.Views;
using PerfectTicketClient.Models.User;
using PerfectTicketClient.Models;
using PerfectTicketClient.Models.Ticket;

namespace PerfectTicketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int ARGC = 5 + 1;
        public UserInfo user { get; set; }
        private bool isAutoMode = false;
        public PerfectEngineClient perfectEngine { get; }

        public TicketLab myTicketLab { get; set; }
        // public TicketLab remainTicketLab { get; set; }
        public TicketRemainLab remainsLab { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            getArguments();
            perfectEngine = new PerfectEngineClient(user, isAutoMode);
        }

        private void getArguments()
        {
            String args = String.Empty;
            StringBuilder strbuilder = new StringBuilder();
            foreach (String arg in Environment.GetCommandLineArgs())
            {
                if (arg.Contains("#"))
                {
                    args = arg;
                }
                strbuilder.AppendLine(arg);
            }
            // args = strbuilder.ToString();
            string[] stringSeparators = new string[] { "#" };
            String[] strs = args.Split(stringSeparators, ARGC, StringSplitOptions.None);
            try
            {
                int userID = Convert.ToInt32(strs[0]);
                String name = strs[1];
                String password = strs[2];
                int priority = Convert.ToInt32(strs[3]);
                int balance = Convert.ToInt32(strs[4]);
                user = new UserInfo(userID, name, password, priority, balance);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            perfectEngine.startEngine();
            myTicketLab = perfectEngine.getMyTicketLab();
            remainsLab = perfectEngine.getRemainLab();
            usernameTextBox.Text = user.name;
            passwordTextBox.Text = user.password;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide controls
            usernameTextBox.Visibility = Visibility.Hidden;
            passwordTextBox.Visibility = Visibility.Hidden;
            loginButton.Visibility = Visibility.Hidden;

            // show dock panel
            dockPanel.Visibility = Visibility.Visible;
            usernameTextBlock.Text = user.name;
            balanceTextBlock.Text = Convert.ToString(user.balance);
            // TODO Authentication

            MyTicketsView myTicketsView = new MyTicketsView();
            // RemainTicketsView remainTicketsView = new RemainTicketsView();
            // set default view is myticketview
            myTicketsView.DataContext = myTicketLab.getTicketList();
            DataContext = myTicketsView;

            ticketUserControl.Visibility = Visibility.Visible;

            // perfectEngine.autoModeScript();
        }

        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO Authentication
        }

        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO Authentication
        }

        private void RemainTicketsView_Clicked(object sender, RoutedEventArgs e)
        {
            RemainTicketsView remainTicketView = new RemainTicketsView();
            perfectEngine.startEngine();
            remainsLab = perfectEngine.getRemainLab();

            remainTicketView.DataContext = remainsLab.getTicketRemainList();
            DataContext = remainTicketView;
            balanceTextBlock.Text = Convert.ToString(user.balance);
        }

        private void MyTicketsView_Clicked(object sender, RoutedEventArgs e)
        {
            MyTicketsView myTicketsView = new MyTicketsView();
            perfectEngine.startEngine();
            myTicketLab = perfectEngine.getMyTicketLab();

            myTicketsView.DataContext = myTicketLab.getTicketList();
            DataContext = myTicketsView;
            balanceTextBlock.Text = Convert.ToString(user.balance);
        }
    }
}
