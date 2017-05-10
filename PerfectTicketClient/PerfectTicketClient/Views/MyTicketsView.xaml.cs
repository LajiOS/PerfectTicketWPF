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
    /// Interaction logic for MyTicketsView.xaml
    /// </summary>
    public partial class MyTicketsView : UserControl
    {
        private static String REFUND_QUESTION = "Are you sure you want to retire this ticket: ";
        private PerfectEngineClient perfectEngine;
        private TicketLab myTicketLab;
        // private TicketLab remainTicketLab;
        private TicketRemainLab remainsLab;
        private TextBlock balanceTextBlock;
        private UserInfo user;

        public MyTicketsView()
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

        private void myTicket_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // MessageBox.Show(Convert.ToString(myTickets.SelectedItems.Count));
            // MessageBox.Show(Convert.ToString(myTickets.SelectedItems[0]));
            TicketInfo selectedTicket = (TicketInfo) myTickets.SelectedItems[0];
            MessageBoxResult result;
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage question = MessageBoxImage.Question;

            result = MessageBox.Show(REFUND_QUESTION+selectedTicket.id, "REFUND", button, question);
            if (result == MessageBoxResult.Yes)
            {
                if (perfectEngine.addRequest(selectedTicket, null, false))
                {
                    MessageBox.Show("Request issued");
                    perfectEngine.startEngine();
                    myTicketLab = perfectEngine.getMyTicketLab();
                    remainsLab = perfectEngine.getRemainLab();
                    DataContext = myTicketLab.getTicketList();
                    balanceTextBlock.Text = Convert.ToString(user.balance);
                } else
                {
                    MessageBox.Show("Request rejected");
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
