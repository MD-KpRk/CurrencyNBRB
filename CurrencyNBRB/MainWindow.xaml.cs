using CurrencyNBRB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CurrencyNBRB
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string CURRENCIES_URL = "https://api.nbrb.by/exrates/currencies";
        private const string RATE_URL = "https://api.nbrb.by/exrates/rates/";
        private const int UpdateSecondCooldown = 10;
        private const int LeftBoxStartNumber = 1;

        private DateTime lastUpdateTime;
        private string? updateDateText = string.Empty;

        private ObservableCollection<Currency> currencies = new ObservableCollection<Currency>();
        
        private Rate? currentRate;
        private Currency? selectedCurrency;
        
        private string? leftBoxString = LeftBoxStartNumber.ToString();
        private bool leftStringCorrect = true;
        private string? rightBoxString;

        public ObservableCollection<Currency> Currencies
        {
            get => currencies;
            set
            {
                currencies = value ?? new ObservableCollection<Currency>();
                OnPropertyChanged(nameof(Currencies));

                if (currencies.Any())
                    SelectedCurrency = currencies.First();
            }
        }

        public Rate? CurrentRate
        {
            get => currentRate;
            set
            {
                currentRate = value;
                OnPropertyChanged(nameof(CurrentRate));
            }
        }

        public Currency? SelectedCurrency
        {
            get => selectedCurrency;
            set
            {
                selectedCurrency = value;
                Task.Run(() => UpdateRate(value));
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        public string? LeftBox
        {
            get => leftBoxString;
            set
            {
                OnPropertyChanged(nameof(LeftBox));
                leftBoxString = value;
                try
                {
                    leftStringCorrect = leftBoxString == Convert.ToDouble(value).ToString();
                }
                catch
                {
                    leftStringCorrect = false;
                }
            }
        }

        public string? RightBox
        {
            get => rightBoxString;
            set
            {
                rightBoxString = value;
                OnPropertyChanged(nameof(RightBox));
            }
        }

        public string UpdateDateText
        {
            get => "Последнее обновление" + "\n" + updateDateText;
            set
            {
                updateDateText = value;
                OnPropertyChanged(nameof(UpdateDateText));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            Init();
            UpdateAll();
        }

        private void Init()
        {
            DataContext = this;
        }

        private void UpdateAll()
        {
            DateTime newDate = DateTime.Now;
            TimeSpan timePassed = newDate - lastUpdateTime;
            if (timePassed.TotalSeconds < UpdateSecondCooldown)
            {
                MessageBox.Show($"Подождите {UpdateSecondCooldown} секунд с момента прошлого обновления.\nОсталось: {(int)(UpdateSecondCooldown - timePassed.TotalSeconds)} секунд");
                return;
            }

            lastUpdateTime = DateTime.Now;
            UpdateDateText = $"{lastUpdateTime.ToShortDateString()} {lastUpdateTime.ToLongTimeString()}";
            Task.Factory.StartNew(UpdateData);
        }

        private async Task UpdateData()
        {
            Currencies = new ObservableCollection<Currency>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(CURRENCIES_URL);
                    Currencies = new ObservableCollection<Currency>(JsonConvert.DeserializeObject<List<Currency>>(response)
                        .Where(c => c.Cur_DateStart <= lastUpdateTime && (c.Cur_DateEnd == null || c.Cur_DateEnd >= lastUpdateTime))
                        .OrderBy(c => c.Cur_Name).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private async Task UpdateRate(Currency? cur)
        {
            if (cur == null) return;
            string apiUrl2 = RATE_URL + cur.Cur_ID;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response2 = await client.GetAsync(apiUrl2);
                    if (response2.IsSuccessStatusCode)
                    {
                        string responseBody2 = await response2.Content.ReadAsStringAsync();
                        CurrentRate = JsonConvert.DeserializeObject<Rate>(responseBody2);
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

        private void RecalculateRate(Rate? rate)
        {
            if (CurrentRate == null) return;
            if (leftStringCorrect && rate != null && rate.Cur_OfficialRate != null)
            {
                double result = Math.Round((rate.Cur_Scale * Convert.ToDouble(LeftBox) / (double)rate.Cur_OfficialRate), 4);
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
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string pattern = $@"^[0-9]*({Regex.Escape(decimalSeparator)}[0-9]*)?$";
                TextBox textBox = (TextBox)sender;
                string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
                if (!Regex.IsMatch(newText, pattern))
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateRate(CurrentRate);
        }
    }
}
