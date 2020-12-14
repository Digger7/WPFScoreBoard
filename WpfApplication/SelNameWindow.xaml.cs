using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlServerCe;
using System.Data;
using System.Windows.Threading;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для SelNameWindow.xaml
    /// </summary>
    public partial class SelNameWindow : Window
    {
        public string name;
        public string team;

        DispatcherTimer _timeInfo;
        const int timerInfoFlicker = 8;
        int timerInfoFlickerValue = 0;

        string connString = "Data Source = " + System.Windows.Forms.Application.StartupPath + "\\Database1.sdf; Persist Security Info=False";

        public SelNameWindow()
        {
            InitializeComponent();
            #region Таймер информационного сообщения
                _timeInfo = new DispatcherTimer();
                _timeInfo.Tick += new EventHandler(delegate(object s, EventArgs a)
                {                    
                    if (labelMessage.Visibility == Visibility.Hidden) {
                        labelMessage.Visibility = Visibility.Visible;
                    }else{
                            labelMessage.Visibility = Visibility.Hidden;
                    };
                    timerInfoFlickerValue++;
                    if (timerInfoFlickerValue == timerInfoFlicker) {
                        labelMessage.Visibility = Visibility.Visible;
                        timerInfoFlickerValue = 0;
                        _timeInfo.Stop();
                    }
                });
                // Установка интервала
                _timeInfo.Interval = TimeSpan.FromMilliseconds(100);
                // Запуск таймера
                //_timeInfo.Start();
                // Остановка таймера
                //_timeInfo.Stop();          
            #endregion

            textBoxNum.Focus();
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void textBoxNum_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    DialogResult = false;
                    break;
                case Key.Enter:
                    Search();
                    break;                
            }
        }

        private void Search() {
            try
            {
                using (var conn = new SqlCeConnection(connString))
                {
                    conn.Open();
                    SqlCeCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT name, team FROM fighters WHERE num = @num";
                    cmd.Parameters.Add("@num", SqlDbType.Int).Value = textBoxNum.Text;
                    SqlCeDataReader myReader = cmd.ExecuteReader();
                    if (myReader.Read())
                    {
                        name = myReader["name"].ToString();
                        team = myReader["team"].ToString();
                        DialogResult = true;
                    }
                    else
                    {
                        textBoxNum.Focus();
                        textBoxNum.SelectAll();
                        labelMessage.Foreground = Brushes.Red;
                        labelMessage.Content = "Не найдено!";
                        _timeInfo.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                textBoxNum.Focus();
                textBoxNum.SelectAll();
                labelMessage.Foreground = Brushes.Red;
                labelMessage.Content = "Не найдено!";
                _timeInfo.Start();
            }
        }

        private void buttonSearch_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    DialogResult = false;
                    break;
                case Key.Enter:
                    Search();
                    break;
            }
        }
    }
}
