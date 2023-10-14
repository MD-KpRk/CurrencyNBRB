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
    }
}
