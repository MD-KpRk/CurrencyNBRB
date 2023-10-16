using System.Windows;
using System.Windows.Controls;

namespace CurrencyNBRB
{
    public partial class CurrencyControl : UserControl
    {
        public string Description { get; set; } = "v";
        public CurrencyControl(string Description)
        {
            this.DataContext = this;
            this.Description = Description;
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show("adwwda");
        }
    }
}
