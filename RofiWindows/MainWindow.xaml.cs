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

namespace RofiWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FilterTextBox.Focus();
            ItemsGrid.ItemsSource = Window.ListWindows();
        }

        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            ItemsGrid.ItemsSource = Window.ListWindows().FilterWindowsByTitleFuzzy(FilterTextBox.Text);
        }

        private void FilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ItemsGrid.Items.Count > 0)
                {
                    if (ItemsGrid.Items[0] is Window w)
                    {
                        w.Activate();
                        Close();
                    }
                }
            }
        }
    }
}
