using System.Collections.ObjectModel;
using System.Windows;

namespace CurrencyNBRB
{
    public partial class MainWindow : Window
    {
        ObservableCollection<CurrencyControl> labels = new ObservableCollection<CurrencyControl>();
        public MainWindow()
        {
            InitializeComponent();
            //DataContext = this;
            myItems.ItemsSource = labels;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            
            labels.Add(new CurrencyControl("CurrencyName"));
        }
    }
}
