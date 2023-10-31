using CurrencyNBRB.Models;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;
using System.Diagnostics;

namespace CurrencyNBRB
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string updatedatetext = string.Empty;
        string apiUrl = "https://api.nbrb.by/exrates/currencies";
        public event PropertyChangedEventHandler? PropertyChanged;
        DateTime dateTime;

        public ObservableCollection<Currency>? Currencies;


        public string? leftboxstring;
        public bool? leftstringcorrect;
        public string? LeftBox
        {
            get { return this.leftboxstring; }
            set
            {
                OnPropertyChanged("LeftBox");
                leftboxstring = value;
                try
                {
                    if (leftboxstring == Convert.ToDouble(value).ToString())
                        leftstringcorrect = true;
                    else
                        leftstringcorrect = false;
                }
                catch
                {
                    leftstringcorrect = false;
                }
            }
        }

        public string? rightboxstring;
        public bool? rightstringcorrect;
        public string? RightBox
        {
            get { return this.rightboxstring; }
            set
            {
                OnPropertyChanged("RightBox");
                rightboxstring = value;
                try
                {
                    if (rightboxstring == Convert.ToDouble(value).ToString())
                        rightstringcorrect = true;
                    else
                        rightstringcorrect = false;
                }
                catch
                {
                    rightstringcorrect = false;
                }
            }
        }

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
            dateTime= DateTime.Now;
            UpdateDateText = dateTime.ToShortDateString() + " " + dateTime.ToLongTimeString();
            Task.Factory.StartNew(UpdateData);
        }

        public async Task UpdateData()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(apiUrl);
                    Currencies = new ObservableCollection<Currency>( JsonConvert.DeserializeObject<List<Currency>>(response)
                        .Where(c => c.Cur_DateStart <= dateTime && (c.Cur_DateEnd == null || c.Cur_DateEnd >= dateTime))
                        .OrderBy(c => c.Cur_Name).ToList());
                    OnPropertyChanged("Currencies");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // CONVERT BUTTON
        {
            //test
                    MessageBox.Show(Currencies.Count().ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Update Button
        {
            Update();
        }

        private void DecimalTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            string decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;
            string pattern = $@"^[0-9]*({Regex.Escape(decimalSeparator)}[0-9]*)?$";

            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            if (!Regex.IsMatch(newText, pattern))
            {
                e.Handled = true;
            }
        }

    }
}
