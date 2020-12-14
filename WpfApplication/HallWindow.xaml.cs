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
using System.Windows.Threading;
using System.Xml;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для HallWindow.xaml
    /// </summary>
    public partial class HallWindow : Window
    {
        DispatcherTimer _timer;

        MainWindow.deathTimerEnum blueDeathTimerTmpVar;
        MainWindow.deathTimerEnum redDeathTimerTmpVar;

        public HallWindow()
        {
            InitializeComponent();

            ReadXML();
            Animate();
            #region Таймер
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
                MainWindow main = this.Owner as MainWindow;
                if (main != null)
                {

                    switch (main.blueDeathTimerVar)
                    {
                        case MainWindow.deathTimerEnum.Start:
                            if (blueDeathTimerTmpVar != MainWindow.deathTimerEnum.Start)
                            {
                                BlueDeathTimerStart();
                                blueDeathTimerTmpVar = MainWindow.deathTimerEnum.Start;
                            }
                            break;
                        case MainWindow.deathTimerEnum.Pause:
                            if (blueDeathTimerTmpVar != MainWindow.deathTimerEnum.Pause)
                            {
                                blueDeathTimerTmpVar = MainWindow.deathTimerEnum.Pause;
                            }
                            break;
                        case MainWindow.deathTimerEnum.Stop:
                            if (blueDeathTimerTmpVar != MainWindow.deathTimerEnum.Stop)
                            {
                                BlueDeathTimerStop();
                                blueDeathTimerTmpVar = MainWindow.deathTimerEnum.Stop;
                            }
                            break;
                    }

                    switch (main.redDeathTimerVar)
                    {
                        case MainWindow.deathTimerEnum.Start:
                            if (redDeathTimerTmpVar != MainWindow.deathTimerEnum.Start)
                            {
                                RedDeathTimerStart();
                                redDeathTimerTmpVar = MainWindow.deathTimerEnum.Start;
                            }
                            break;
                        case MainWindow.deathTimerEnum.Pause:
                            if (redDeathTimerTmpVar != MainWindow.deathTimerEnum.Pause)
                            {
                                redDeathTimerTmpVar = MainWindow.deathTimerEnum.Pause;
                            }
                            break;
                        case MainWindow.deathTimerEnum.Stop:
                            if (redDeathTimerTmpVar != MainWindow.deathTimerEnum.Stop)
                            {
                                RedDeathTimerStop();
                                redDeathTimerTmpVar = MainWindow.deathTimerEnum.Stop;
                            }
                            break;
                    }

                    // Обновление содержимого элементов управления -->
                    Time.Text = main.Time.Text;
                    BlueName.Text = main.BlueName.Text;
                    RedName.Text = main.RedName.Text;
                    Weight.Text = main.Weight.Text;
                    BlueScore.Text = main.BlueScore.Text;
                    RedScore.Text = main.RedScore.Text;
                    BlueActivityText.Text = main.BlueActivityText.Text;
                    RedActivityText.Text = main.RedActivityText.Text;
                    BlueTechTime.Text = main.BlueTechTime.Text;
                    RedTechTime.Text = main.RedTechTime.Text;
                    PauseBlueTechTime.Visibility = main.PauseBlueTechTime.Visibility;
                    PauseRedTechTime.Visibility = main.PauseRedTechTime.Visibility;

                    BlueTimeDeath.Text = main.BlueTimeDeath.Text;
                    RedTimeDeath.Text = main.RedTimeDeath.Text;
                    BlueTimeDeathGrid.Visibility = main.BlueTimeDeathGrid.Visibility;
                    RedTimeDeathGrid.Visibility = main.RedTimeDeathGrid.Visibility;
                    PauseBlueTimeDeath.Visibility = main.PauseBlueTimeDeath.Visibility;
                    PauseRedTimeDeath.Visibility = main.PauseRedTimeDeath.Visibility;

                    PauseTime.Visibility = main.PauseTime.Visibility;
                    BlueWarningText.Text = main.BlueWarningText.Text;
                    RedWarningText.Text = main.RedWarningText.Text;

                    labelTeamRed.Content = main.textBoxTeamRed.Text;
                    labelTeamBlue.Content = main.textBoxTeamBlue.Text;

                    SetRedScoreFontSize();
                    SetBlueScoreFontSize();

                    if (main.runningLineRestart == true)
                    {//Обновление бегущей строки в случае сохранения параметров в главной форме
                        ReadXML();
                        Animate();
                        main.runningLineRestart = false;
                    }
                    // Обновление содержимого элементов управления --<
                }
            });
            // Установка интервала
            _timer.Interval = TimeSpan.FromMilliseconds(1);
            // Запуск таймера
            _timer.Start();
            // Остановка таймера
            //_timer.Stop();          
            #endregion
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow main = this.Owner as MainWindow;
            if(main != null)
            {
                main.hallWindowOpen = false;
            }
        }

        private void GoKeyDown(object sender, KeyEventArgs e)
        {
            #region включение/выключение полноэкранного режима
                if (e.Key == Key.F11)
                {//включение\выключение полноэкранного режима
                    fullscreen();
                }
            #endregion
        }

        private void Main_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            fullscreen();
        }

        private void fullscreen()
        {//--> включение\выключение полноэкранного режима
            if (this.WindowStyle != WindowStyle.None)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                this.ResizeMode = ResizeMode.CanResize;
            }
        }//--< включение\выключение полноэкранного режима

        private void Animate()
        {
            #region Вычисление ширины бегущей строки. Запуск анимации.
            double textWidth = 0;
            Typeface typeface = new Typeface(RunningLine.FontFamily,
                RunningLine.FontStyle, RunningLine.FontWeight, RunningLine.FontStretch);
            GlyphTypeface glyphTypeface;
            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                throw new InvalidOperationException("No glyphtypeface found");
            double size = RunningLine.FontSize;
            ushort[] glyphIndexes = new ushort[RunningLine.Text.Length];
            double[] advanceWidths = new double[RunningLine.Text.Length];

            for (int n = 0; n < RunningLine.Text.Length; n++)
            {
                ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[RunningLine.Text[n]];
                glyphIndexes[n] = glyphIndex;

                double width = glyphTypeface.AdvanceWidths[glyphIndex] * size;
                advanceWidths[n] = width;

                textWidth += width;
            }

            //-----------------------------------------------------------------------------
            double tmpD = RunningLine.Text.Length / 38;//Изврат
            RunningLineAnimation.SpeedRatio = 1 / (tmpD > 0 ? tmpD : 1);//бубен с анимацией
            //-----------------------------------------------------------------------------
            RunningLineAnimation.From = this.Width + 150;
            RunningLineAnimation.To = textWidth * -1;
            RunningLineAnimation.Duration = TimeSpan.FromSeconds(20);//установка скорости анимации 
            RunningLineStroy.Begin(this, true);
            //RunningLineStroy.Storyboard.Stop(this);
            #endregion
        }

        private void ReadXML()
        {
            #region Чтение бегущей строки и клавиш из XML
            try
            {
                RunningLine.Text = "";
                XmlDocument doc = new XmlDocument();
                string FileName = System.Windows.Forms.Application.StartupPath + "\\custom.xml";
                doc.Load(FileName);
                foreach (XmlNode table in doc.DocumentElement.ChildNodes)
                {
                    #region перебираем все атрибуты элемента
                    //foreach (XmlAttribute attr in table.Attributes)
                    //{
                    //    MessageBox.Show(attr.Name + " : " + attr.Value);
                    //}
                    #endregion
                    // перебираем всех чилдов текущего узла parentNode.AppendChild(node);
                    foreach (XmlNode ch in table.ChildNodes)
                    {
                        switch (ch.ParentNode.Name)
                        {
                            case "Text":
                                RunningLine.Text = ch.Value;
                                break;
                        }
                    }
                    // Получаем текст в текущем узле
                    // MessageBox.Show(table.InnerText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion
        }

        private void RedDeathAnimation_Completed(object sender, EventArgs e)
        {
            RedTimeDeathGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void BlueDeathAnimation_Completed(object sender, EventArgs e)
        {
            BlueTimeDeathGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void RedDeathTimerStop()
        {
            PauseRedTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            RedDeathAnimation.From = 384;
            RedDeathAnimation.To = 628;
            RedDeathAnim.Begin(this, true);
            ResetTimeRedDeath();
        }
        private void BlueDeathTimerStop()
        {
            PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            BlueDeathAnimation.From = 64;
            BlueDeathAnimation.To = -180;
            BlueDeathAnim.Begin(this, true);
            ResetTimeBlueDeath();
        }

        private void ResetTimeBlueDeath()
        {
            BlueTimeDeath.Text = "0:00";
        }

        private void ResetTimeRedDeath()
        {
            RedTimeDeath.Text = "0:00";
        }

        private void SetRedScoreFontSize()
        {
            switch (RedScore.Text.Length)
            {
                default:
                    RedScore.FontSize = 80;
                    break;
                case 4:
                    RedScore.FontSize = 64;
                    break;
                case 5:
                    RedScore.FontSize = 50;
                    break;
            }
        }

        private void SetBlueScoreFontSize()
        {
            switch (BlueScore.Text.Length)
            {
                default:
                    BlueScore.FontSize = 80;
                    break;
                case 4:
                    BlueScore.FontSize = 64;
                    break;
                case 5:
                    BlueScore.FontSize = 50;
                    break;
            }
        }

        private void BlueDeathTimerStart()
        {
            if (PauseBlueTimeDeath.Visibility != System.Windows.Visibility.Visible)
            {
                BlueTimeDeathGrid.Visibility = System.Windows.Visibility.Visible;
                BlueDeathAnimation.From = -180;
                BlueDeathAnimation.To = 64;
                BlueDeathAnim.Begin(this, true);            
            }
        }

        private void RedDeathTimerStart()
        {
            if (PauseRedTimeDeath.Visibility != System.Windows.Visibility.Visible)
            {
                RedTimeDeathGrid.Visibility = System.Windows.Visibility.Visible;
                RedDeathAnimation.From = 628;
                RedDeathAnimation.To = 384;
                RedDeathAnim.Begin(this, true);
            }
        }
    }
}
