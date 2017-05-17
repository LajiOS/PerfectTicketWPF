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

using PerfectTicketClient.Models.Ticket;
using PerfectTicketClient.Models;
using PerfectTicketClient.Models.User;

namespace PerfectTicketClient.Views
{
    /// <summary>
    /// Interaction logic for RemainTicketsView.xaml
    /// </summary>
    public partial class RemainTicketsView : UserControl
    {
        private static String BUY_QUESTION = "Are you sure you want to buy ticket: ";
        private PerfectEngineClient perfectEngine;
        private TicketLab myTicketLab;
        // private TicketLab remainTicketLab;
        private TicketRemainLab remainsLab;
        private TextBlock balanceTextBlock;
        private UserInfo user;

        public RemainTicketsView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
                if (parentWindow != null)
                {
                    perfectEngine = parentWindow.perfectEngine;
                    myTicketLab = parentWindow.myTicketLab;
                    // remainTicketLab = parentWindow.remainTicketLab;
                    remainsLab = parentWindow.remainsLab;
                    balanceTextBlock = parentWindow.balanceTextBlock;
                    user = parentWindow.user;
                }
            };
        }
        private void remainTicket_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            TicketRemain selectedRemain = (TicketRemain)remainTickets.SelectedItems[0];
            MessageBoxResult result;
            MessageBoxButton yesnoButton = MessageBoxButton.YesNo;
            MessageBoxButton okButton = MessageBoxButton.OK;
            MessageBoxImage question = MessageBoxImage.Question;

            result = MessageBox.Show(BUY_QUESTION+" from " + selectedRemain.start + " to " + selectedRemain.terminal, "BUY", yesnoButton, question);
            if (result == MessageBoxResult.Yes)
            {
                if (perfectEngine.addRequest(null, selectedRemain, true))
                {
                    MessageBox.Show("Request issued.", "Client", okButton);
                    perfectEngine.startEngine();
                    myTicketLab = perfectEngine.getMyTicketLab();
                    // remainTicketLab = perfectEngine.getRemainTicketLab();
                    remainsLab = perfectEngine.getRemainLab();
                    DataContext = remainsLab.getTicketRemainList();
                    balanceTextBlock.Text = Convert.ToString(user.balance);
                }
                else
                {
                    MessageBox.Show("Request rejected.", "Client", okButton);
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
