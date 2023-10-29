using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CurrencyNBRB
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string updatedatetext = string.Empty;
        string apiUrl = "https://api.nbrb.by/exrates/currencies";
        public event PropertyChangedEventHandler? PropertyChanged;

        public string UpdateDateText
        {
            get
            {
                return "Последнее обновление" + "\n" + updatedatetext;
            }
            set
            {
                updatedatetext = value;
                OnPropertyChanged("UpdateDateText");
            }
        }
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Init();
            Update();
        }

        public void Init()
        {
            DataContext = this;

        }

        public void Update()
        {
            DateTime dateTime= DateTime.Now;
            UpdateDateText = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //test
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Update Button
        {
            Update();
        }
    }
}
