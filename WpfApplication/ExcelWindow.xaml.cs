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
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для ExcelWindow.xaml
    /// </summary>
    public partial class ExcelWindow : Window
    {
        BackgroundWorker bw_load;
        BackgroundWorker bw_getData;

        DataView data;

        bool loadComplete = false;

        string connString = "Data Source = " + System.Windows.Forms.Application.StartupPath + "\\Database1.sdf; Persist Security Info=False";

        public ExcelWindow()
        {
            InitializeComponent();
            labelInfo.Visibility = Visibility.Hidden;

            #region GetData
                bw_getData = new BackgroundWorker();
                bw_getData.DoWork += delegate(object sender, DoWorkEventArgs e)
                {
                    //##################################
                    System.Threading.Thread.Sleep(500);
                    //##################################
                    // BODY -->
                    SqlCeConnection con = new SqlCeConnection();
                    SqlCeDataAdapter ad = new SqlCeDataAdapter();
                    SqlCeCommand cmd = new SqlCeCommand();
                    String str = "SELECT num, name, team FROM fighters";
                    cmd.CommandText = str;
                    ad.SelectCommand = cmd;
                    con.ConnectionString = connString;
                    cmd.Connection = con;
                    DataSet ds = new DataSet();
                    ad.Fill(ds);
                    data = ds.Tables[0].DefaultView;
                    con.Close();
                    // BODY --<
                };

                bw_getData.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Cancelled == true)
                    {

                    }
                    else if (e.Error != null)
                    {

                    }
                    else
                    {
                        lvDataBinding.DataContext = data;
                        if (!loadComplete) labelInfo.Visibility = Visibility.Hidden;
                    }
                };
                labelInfo.Content = String.Format("Чтение из базы данных");
                labelInfo.Foreground = Brushes.Blue;
                labelInfo.Visibility = Visibility.Visible;
                bw_getData.RunWorkerAsync();
            #endregion

            //создаем экземпляр класса BackgroundWorker
            bw_load = new BackgroundWorker();
            //объект BackgroundWorker поддерживает обновление сведений о ходе выполнения
            bw_load.WorkerReportsProgress = true;
            bw_load.WorkerSupportsCancellation = true;
            //описание непосредственно работы потока
            bw_load.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                    //##################################
                    System.Threading.Thread.Sleep(500);
                    //##################################
                    // BODY -->
                        #region BODY
                            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                            dlg.DefaultExt = ".xls";
                            dlg.Filter = "Excel documents (.xls)|*.xls";
                            Nullable<bool> result = dlg.ShowDialog();
                            if (result == true)
                            {
                                // -- Очистка таблицы -->
                                using (var conn = new SqlCeConnection(connString))
                                {
                                    conn.Open();
                                    SqlCeCommand cmd = conn.CreateCommand();
                                    cmd.CommandText = "DELETE FROM fighters WHERE 1=1";
                                    cmd.ExecuteNonQuery();
                                }
                                // -- Очистка таблицы --<

                                string path = dlg.FileName;
                                var ds = new DataSet("EXCEL");
                                var con = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0", path));
                                try
                                {
                                    con.Open();
                                    DataTable schemaTable = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });//Получаем список листов в файле
                                    if (schemaTable != null)
                                    {
                                        var sheet1 = (string)schemaTable.Rows[0].ItemArray[2];
                                        string select = String.Format("SELECT * FROM [{0}]", sheet1);
                                        var ad = new OleDbDataAdapter(@select, con);
                                        ad.Fill(ds);
                                    }
                                    var tb = ds.Tables[0];

                                    bool wasAError = false;
                                    string errorText = string.Empty;

                                    int rowCount = tb.Rows.Count;
                                    for (int j = 0; j < tb.Rows.Count; j++)
                                    {//Строки

                                        if (bw_load.CancellationPending) break;

                                        bool error = false;

                                        string num = string.Empty;
                                        string name = string.Empty;
                                        string team = string.Empty;

                                        for (int i = 0; i < tb.Rows[j].ItemArray.Count(); i++)
                                        {//Столбцы
                                            string strValue = tb.Rows[j].ItemArray[i].ToString().Trim();

                                            switch (i)
                                            {//Номер столбца
                                                case 0:
                                                    #region НОМЕР
                                                    num = strValue;
                                                    if (String.IsNullOrEmpty(strValue))
                                                    {
                                                        error = true;
                                                        string message = "Значение ID не заполнено (строка " + j + "); ";
                                                        if (errorText.IndexOf(message, System.StringComparison.Ordinal) == -1)
                                                        {
                                                            errorText += message + "\r\n";
                                                        };
                                                    }
                                                    #endregion
                                                    break;
                                                case 1:
                                                    #region ФАМИЛИЯ
                                                    name = strValue;
                                                    if (String.IsNullOrEmpty(strValue))
                                                    {
                                                        error = true;
                                                        string message = "Значение с ИМЕНЕМ не заполнено (строка " + j + "); ";
                                                        if (errorText.IndexOf(message, System.StringComparison.Ordinal) == -1)
                                                        {
                                                            errorText += message + "\r\n";
                                                        };
                                                    }
                                                    #endregion
                                                    break;
                                                case 2:
                                                    #region КОМАНДА
                                                    team = strValue;
                                                    if (String.IsNullOrEmpty(strValue))
                                                    {
                                                        error = true;
                                                        string message = "Значение с КОМАНДОЙ не заполнено (строка " + j + "); ";
                                                        if (errorText.IndexOf(message, System.StringComparison.Ordinal) == -1)
                                                        {
                                                            errorText += message + "\r\n";
                                                        };
                                                    }
                                                    #endregion
                                                    break;
                                            }
                                        }
                                        if (!error)
                                        { //Вставка данных
                                            using (var conn = new SqlCeConnection(connString))
                                            {
                                                var insertCommand = new SqlCeCommand("INSERT INTO fighters (num, name, team) VALUES (@num, @name, @team);", conn);
                                                insertCommand.Parameters.Add("@num", SqlDbType.Int);
                                                insertCommand.Parameters["@num"].Value = num;
                                                insertCommand.Parameters.Add("@name", SqlDbType.NVarChar);
                                                insertCommand.Parameters["@name"].Value = name;
                                                insertCommand.Parameters.Add("@team", SqlDbType.NVarChar);
                                                insertCommand.Parameters["@team"].Value = team;
                                                conn.Open();
                                                var insertResult = insertCommand.ExecuteNonQuery();
                                                /*
                                                if (insertResult == 1)
                                                {
                                                    insertCommand.CommandText = "SELECT @@IDENTITY";
                                                    var retval = insertCommand.ExecuteScalar();
                                                    if (retval != DBNull.Value)
                                                    {
                                                        //MessageBox.Show(String.Format("Добавлена запись, ID: {0}", retval));
                                                    }
                                                }
                                                */
                                            }


                                        };
                                        if (error) wasAError = true;

                                        bw_load.ReportProgress(Convert.ToInt32(Math.Round(Convert.ToDecimal(j / (rowCount / 100)))));
                                    }

                                    con.Close();
                                    if (wasAError)
                                    {
                                        //System.Windows.Forms.MessageBox.Show("В процессе загрузки были ошибки:\r\n" + errorText);
                                    }
                                    else
                                    {
                                        loadComplete = true;
                                        //System.Windows.Forms.MessageBox.Show("Была произведена загрузка данных");
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        #endregion
                    // BODY --<
                    //##################################
            };


            //вывод результатов работы на экран
            bw_load.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                labelInfo.Content = "Загрузка...";
                labelInfo.Foreground = Brushes.Green;
                labelInfo.Visibility = Visibility.Visible;
                progressBar1.Value = e.ProgressPercentage;
            };

            //обработчик события по завершению работы потока
            bw_load.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Cancelled == true)
                {
                    labelInfo.Content = "Отменено";
                    labelInfo.Foreground = Brushes.Red;
                    labelInfo.Visibility = Visibility.Visible;
                }
                else if (e.Error != null)
                {
                    labelInfo.Content = String.Format("Ошибка: {0}", e.Error.Message);
                    labelInfo.Foreground = Brushes.Red;
                    labelInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    //GetData
                    if (!bw_getData.IsBusy) bw_getData.RunWorkerAsync();
                    if (loadComplete)
                    {
                        labelInfo.Content = String.Format("Загрузка выполнена в {0}", DateTime.Now.ToLongTimeString());
                        labelInfo.Foreground = Brushes.Green;
                        labelInfo.Visibility = Visibility.Visible;
                    }
                    else {
                        labelInfo.Visibility = Visibility.Hidden;
                    }
                }
            };
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!bw_load.IsBusy)
            {
                bw_load.RunWorkerAsync();
            }
        }
    }

}
