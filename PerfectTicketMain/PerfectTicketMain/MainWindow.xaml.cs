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
using System.Threading;

using PerfectTicketMain.Models.User;
using PerfectTicketMain.Models.Ticket;
using PerfectTicketMain.Views;
using PerfectTicketMain.Models;
using System.ComponentModel;

namespace PerfectTicketMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        private PerfectTicketEngine perfectTicket;
        private UserLab userLab;
        private TicketLab ticketLab;

        private int userNum;
        private static int DEFAULT_USER_NUM = 3;

        private Thread loadFilesThread;


        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);


            // init a perfectticketengine
            perfectTicket = new PerfectTicketEngine();
            // create load files thread
            loadFilesThread = new Thread(new ThreadStart(loadFilesThreadMethod));
            loadFilesThread.Start();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // get ticket lab user lab
            ticketLab = perfectTicket.getTicketLab();
            userLab = perfectTicket.getUserLab();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            perfectTicket.closeEngine();
        }

        private void userNumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                userNum = Int32.Parse(userNumTextBox.Text);
            } catch (Exception error)
            {
                Console.WriteLine(error.Message);
                userNum = DEFAULT_USER_NUM;
            }
        }

        private void Start_Clicked(object sender, RoutedEventArgs e)
        {
            // Engine start
            perfectTicket.startEngine(userNum);

            // show buttons
            userViewButton.Visibility = Visibility.Visible;
            ticketViewButton.Visibility = Visibility.Visible;
            produceTicketButton.Visibility = Visibility.Visible;

            // hide controls
            userNumTextBox.IsEnabled = false;
            startButton.IsEnabled = false;
        }
        private void UserView_Clicked(object sender, RoutedEventArgs e)
        {
            UserView userView = new UserView();
            userView.DataContext = userLab.getUserList();
            DataContext = userView;
        }

        private void TicketView_Clicked(object sender, RoutedEventArgs e)
        {
            TicketView ticketView = new TicketView();
            ticketView.DataContext = ticketLab.getTicketList();
            DataContext = ticketView;
        }

        private void produceTicketButton_Clicked(object sender, RoutedEventArgs e)
        {
            NewTicketWindow newTicketW = new NewTicketWindow();
            newTicketW.Owner = this;
            newTicketW.ShowDialog();
            MessageBoxButton okButton = MessageBoxButton.OK;

            if (newTicketW.valueIsOK) {
                    int start = newTicketW.start;
                    int terminal = newTicketW.terminal;

                    if (perfectTicket.producerAddRequest(start, terminal))
                    {
                        MessageBox.Show("Request issued", "Main", okButton);
                        perfectTicket.refreshTicketData();
                        ticketLab = perfectTicket.getTicketLab();
                    }
                }
        }

        // SUB THREAD ---
        private void loadFilesThreadMethod()
        {
            // read all files
            perfectTicket.readFiles();
        }
        // ---|
    }
}
