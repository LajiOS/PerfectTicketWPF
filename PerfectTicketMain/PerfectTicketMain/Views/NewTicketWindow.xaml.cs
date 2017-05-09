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
using System.Windows.Shapes;

namespace PerfectTicketMain.Views
{
    /// <summary>
    /// Interaction logic for NewTicketWindow.xaml
    /// </summary>
    public partial class NewTicketWindow : Window
    {
        public int start { get; set; }
        public int terminal { get; set; }
        public bool valueIsOK { get; set; }
        public NewTicketWindow()
        {
            InitializeComponent();
            valueIsOK = false;
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                start = Convert.ToInt32(startTextBox.Text);
                terminal = Convert.ToInt32(terminalTextBox.Text);
            } catch(Exception error)
            {
                Console.WriteLine(error.Message);
                MessageBox.Show(error.Message + "\nPlease input valid value!");
            }
            if (start < 0 || start > 9 || terminal < 0 || terminal > 9)
            {
                MessageBox.Show("Please input valid value!");
            } else
            {
                valueIsOK = true;
                this.Close();
            }

        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
