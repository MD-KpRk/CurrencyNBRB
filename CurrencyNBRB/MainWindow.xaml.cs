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
using System.Globalization;

namespace CurrencyNBRB
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string updatedatetext = string.Empty;
        string apiUrl = "https://api.nbrb.by/exrates/currencies";
        public event PropertyChangedEventHandler? PropertyChanged;
        public int UpdateSecondCooldown = 10;
        DateTime LastUpdateTime;
        private ObservableCollection<Currency>? _Currencies;
        public ObservableCollection<Currency>? Currencies
        {
            get { return _Currencies; }
            set 
            { 
                _Currencies = value; 
                OnPropertyChanged(nameof(Currencies));

                if(_Currencies != null)
                    SelectedCurrency = _Currencies.First();
            }
        }

        public Rate? _CurrentRate;
        public Rate? CurrentRate
        {
            get => _CurrentRate;
            set
            {
                _CurrentRate = value;
                OnPropertyChanged(nameof(CurrentRate));
            }
        }

        public Currency? _SelectedCurrency;
        public Currency? SelectedCurrency
        {
            get => _SelectedCurrency;
            set
            {
                _SelectedCurrency = value;
                Task.Run(() => UpdateRate(value));
                OnPropertyChanged("SelectedCurrency");

            }
        }

        public string? leftboxstring = "1";
        public bool leftstringcorrect = true;
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
                OnPropertyChanged("RightBox");
            }
        }

        public string UpdateDateText
        {
            get => "Последнее обновление" + "\n" + updatedatetext;
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
                handler(this, new PropertyChangedEventArgs(info));
        }

        public MainWindow()
        {
            InitializeComponent();
            Init();
            UpdateAll();
        }

        public void Init()
        {
            DataContext = this;
        }

        public void UpdateAll()
        {
            DateTime newDate = DateTime.Now;
            TimeSpan timePassed = newDate - LastUpdateTime;
            if (timePassed.TotalSeconds < UpdateSecondCooldown)
            {
                MessageBox.Show("Подождите "+ UpdateSecondCooldown + " секунд с момента прошлого обновления.\nОсталось: " + (int)(UpdateSecondCooldown - timePassed.TotalSeconds) + " секунд");
                return;
            }
            LastUpdateTime = DateTime.Now;
            UpdateDateText = LastUpdateTime.ToShortDateString() + " " + LastUpdateTime.ToLongTimeString();
            Task.Factory.StartNew(UpdateData);
        }

        public async Task UpdateData()
        {
            Currencies = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(apiUrl);
                    Currencies = new ObservableCollection<Currency>( JsonConvert.DeserializeObject<List<Currency>>(response)
                        .Where(c => c.Cur_DateStart <= LastUpdateTime && (c.Cur_DateEnd == null || c.Cur_DateEnd >= LastUpdateTime))
                        .OrderBy(c => c.Cur_Name).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        public async Task UpdateRate(Currency? Cur)
        {
            if (Cur == null) return;
            string apiUrl2 = "https://api.nbrb.by/exrates/rates/" + Cur.Cur_ID;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response2 = await client.GetAsync(apiUrl2);
                    if (response2.IsSuccessStatusCode)
                    {
                        string responseBody2 = await response2.Content.ReadAsStringAsync();
                        CurrentRate = Newtonsoft.Json.JsonConvert.DeserializeObject<Rate>(responseBody2);
                        RecalculateRate(CurrentRate);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: {response2.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        public void RecalculateRate(Rate? rate)
        {
            if (CurrentRate == null ) return;
            if (leftstringcorrect && rate != null && rate.Cur_OfficialRate != null)
            {
                double result = Math.Round(((double)rate.Cur_OfficialRate / rate.Cur_Scale * Convert.ToDouble(LeftBox)), 4);
                RightBox = result.ToString();
                OnPropertyChanged(nameof(RightBox));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Update Button
        {
            UpdateAll();
        }

        private void DecimalTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                string decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;
                string pattern = $@"^[0-9]*({Regex.Escape(decimalSeparator)}[0-9]*)?$";
                TextBox textBox = (TextBox)sender;
                string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
                if (!Regex.IsMatch(newText, pattern))
                    e.Handled = true;
            }
            catch (Exception ex) { MessageBox.Show($"Произошла ошибка: {ex.Message}"); }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateRate(CurrentRate);
        }
    }
}
