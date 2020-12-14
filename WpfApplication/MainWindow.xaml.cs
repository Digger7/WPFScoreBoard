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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using WPFCSharpWebCam;
using System.Media;
using System.Windows.Resources;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region Клавиши управления по умолчанию
        //###################################################################
        //Определение клавиш управления, если произойдет ошибка чтения xml файла (в нем пользователь может переназначить управление)
        //,то будут использоваться клавиши по умолчанию определённые ниже:
        string RedPoints = "Q", BluePoints = "W", ResetBluePoints = "R", ResetRedPoints = "E";
        string RedWarning = "A", BlueWarning = "S", ResetBlueWarning = "F", ResetRedWarning = "D";
        string RedActivity = "Z", BlueActivity = "X", ResetBlueActivity = "V", ResetRedActivity = "C";
        string StartPauseTheTimer = "L", ResumeTimer = "P";
        string StartPauseTheTimerRedDoctor = "I", ResetTheTimerRedDoctor = "O";
        string StartPauseTheTimerBlueDoctor = "J", ResetTheTimerBlueDoctor = "K";
        string BlueBonus = "Y", RedBonus = "T";
        string BlueDeath = "H", RedDeath = "G";
        string StopBlueDeath = "N", StopRedDeath = "B";
        //###################################################################  
        #endregion

        bool pressKeyLock = false;

        SoundPlayer player = new SoundPlayer();
        StreamResourceInfo sri = Application.GetResourceStream(new Uri("Sounds/sirena.wav",UriKind.Relative));

        public bool hallWindowOpen = false;
        public bool runningLineRestart = false;

        //----------------------------------------- Для работы анимации таймеров
        public enum deathTimerEnum { None, Start, Pause, Stop };
        public deathTimerEnum blueDeathTimerVar = deathTimerEnum.None;
        public deathTimerEnum redDeathTimerVar = deathTimerEnum.None;
        //-----------------------------------------

        const int maxScore = 9999;

        bool allDeath = false;

        int ResetMinutes = 5;
        int ResetSeconds = 0;
        int Minutes;
        int Seconds;
        int RedTechMinutes;
        int RedTechSeconds;
        int BlueTechMinutes;
        int BlueTechSeconds;
        int RedDeathMinutes;
        int RedDeathSeconds;
        int BlueDeathMinutes;
        int BlueDeathSeconds;
        DispatcherTimer _timer;
        DispatcherTimer _timer_RedDoctor;
        DispatcherTimer _timer_BlueDoctor;
        DispatcherTimer _timer_RedDeath;
        DispatcherTimer _timer_BlueDeath;

        public MainWindow()
        {
            //SplashScreen splashScreen = new SplashScreen("loading.png");
            //splashScreen.Show(true);

            InitializeComponent();

            textBoxTeamRed.Text = "";
            textBoxTeamBlue.Text = "";

            #region PopupMenu
            //---------------------------------------------------------------->
                ContextMenu PopupMenu = new ContextMenu();

                MenuItem ItemPopup = new MenuItem();
                ItemPopup.Header = "Блокировать текст";
                ItemPopup.IsCheckable = true;
                ItemPopup.Click += new RoutedEventHandler(ItemPopup_Click_BlockText);
                PopupMenu.Items.Add(ItemPopup);                
            
                ItemPopup = new MenuItem();
                ItemPopup.Header = "Время";
                ItemPopup.Click += new RoutedEventHandler(ItemPopup_Click_Time);
                PopupMenu.Items.Add(ItemPopup);

                ItemPopup = new MenuItem();
                ItemPopup.Header = "Параметры";
                ItemPopup.Click += new RoutedEventHandler(ItemPopup_Click_Param);
                PopupMenu.Items.Add(ItemPopup);

                this.ContextMenu = PopupMenu;
            //----------------------------------------------------------------<
            #endregion
            ResetTime();
            ReadXML();//Чтение бегущей строки и клавиш из XML
            Animate();
            #region Таймер
                _timer = new DispatcherTimer();
                _timer.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    Seconds--;
                    if (Seconds <= 0 && Minutes != 0)
                    {                        
                        Seconds = 59;
                        Minutes--;
                    }
                    string tmp = "0" + Seconds.ToString();
                    Time.Text = Minutes+":"+tmp.Substring(tmp.Length-2,2);
                    if (Seconds == 0 && Minutes == 0) 
                    {
                        Time.Text = "0:00";
                        _timer.Stop();
                        //SoundPlayer player = new SoundPlayer();
                        //StreamResourceInfo sri = Application.GetResourceStream(new Uri("Sounds/sirena.wav",UriKind.Relative));
                        //player.Stream = sri.Stream;
                        player.Stream = sri.Stream;
                        player.Play();
                        PlayTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
                    };
                });
                // Установка интервала
                _timer.Interval = TimeSpan.FromMilliseconds(1000);
                // Запуск таймера
                //_timer.Start();
                // Остановка таймера
                //_timer.Stop();          
            #endregion
            #region Таймер доктора красного участника
                _timer_RedDoctor = new DispatcherTimer();
                _timer_RedDoctor.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    RedTechSeconds++;
                    if (RedTechSeconds >= 60)
                    {
                        RedTechSeconds = 0;
                        RedTechMinutes++;
                    }
                    string tmp = "0" + RedTechSeconds.ToString();
                    RedTechTime.Text = RedTechMinutes + ":" + tmp.Substring(tmp.Length - 2, 2);
                    if (RedTechMinutes == 60) // Час у врача - это круто, стопим таймер))
                    {
                        ResetTimeRedTech();
                    };
                });
                // Установка интервала
                _timer_RedDoctor.Interval = TimeSpan.FromMilliseconds(1000);        
            #endregion
            #region Таймер доктора синего участника
                _timer_BlueDoctor = new DispatcherTimer();
                _timer_BlueDoctor.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    BlueTechSeconds++;
                    if (BlueTechSeconds >= 60)
                    {
                        BlueTechSeconds = 0;
                        BlueTechMinutes++;
                    }
                    string tmp = "0" + BlueTechSeconds.ToString();
                    BlueTechTime.Text = BlueTechMinutes + ":" + tmp.Substring(tmp.Length - 2, 2);
                    if (BlueTechMinutes == 60) // Час у врача - это круто, стопим таймер))
                    {
                        ResetTimeBlueTech();
                    };
                });
                // Установка интервала
                _timer_BlueDoctor.Interval = TimeSpan.FromMilliseconds(1000); 
             #endregion
            #region Таймер болевого удушения синего участника
                _timer_BlueDeath = new DispatcherTimer();
                _timer_BlueDeath.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    BlueDeathSeconds++;
                    if (BlueDeathSeconds >= 60)
                    {
                        BlueDeathSeconds = 0;
                        BlueDeathMinutes++;
                    }
                    string tmp = "0" + BlueDeathSeconds.ToString();
                    BlueTimeDeath.Text = BlueDeathMinutes + ":" + tmp.Substring(tmp.Length - 2, 2);
                    if (BlueDeathSeconds > 20) // 20 секунд удушения - предел)
                    {
                        BlueDeathTimerStop();
                    };
                });
                // Установка интервала
                _timer_BlueDeath.Interval = TimeSpan.FromMilliseconds(1000);
            #endregion
            #region Таймер болевого удушения красного участника
                _timer_RedDeath = new DispatcherTimer();
                _timer_RedDeath.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    RedDeathSeconds++;
                    if (RedDeathSeconds >= 60)
                    {
                        RedDeathSeconds = 0;
                        RedDeathMinutes++;
                    }
                    string tmp = "0" + RedDeathSeconds.ToString();
                    RedTimeDeath.Text = RedDeathMinutes + ":" + tmp.Substring(tmp.Length - 2, 2);
                    if (RedDeathSeconds > 20) // 20 секунд удушения - предел)
                    {
                        RedDeathTimerStop();
                    };
                });
                // Установка интервала
                _timer_RedDeath.Interval = TimeSpan.FromMilliseconds(1000);
                #endregion
        }

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
            double tmpD  = RunningLine.Text.Length/38;//Изврат
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
                            case "RedPoints":
                                RedPoints = ch.Value;
                                break;
                            case "BluePoints":
                                BluePoints = ch.Value;
                                break;
                            case "ResetBluePoints":
                                ResetBluePoints = ch.Value;
                                break;
                            case "ResetRedPoints":
                                ResetRedPoints = ch.Value;
                                break;
                            case "RedWarning":
                                RedWarning = ch.Value;
                                break;
                            case "BlueWarning":
                                BlueWarning = ch.Value;
                                break;
                            case "ResetBlueWarning":
                                ResetBlueWarning = ch.Value;
                                break;
                            case "ResetRedWarning":
                                ResetRedWarning = ch.Value;
                                break;
                            case "StartPauseTheTimer":
                                StartPauseTheTimer = ch.Value;
                                break;
                            case "ResumeTimer":
                                ResumeTimer = ch.Value;
                                break;
                            case "StartPauseTheTimerRedDoctor":
                                StartPauseTheTimerRedDoctor = ch.Value;
                                break;
                            case "ResetTeheTimerRedDoctor":
                                ResetTheTimerRedDoctor = ch.Value;
                                break;
                            case "StartPauseTheTimerBlueDoctor":
                                StartPauseTheTimerBlueDoctor = ch.Value;
                                break;
                            case "ResetTeheTimerBlueDoctor":
                                ResetTheTimerBlueDoctor = ch.Value;
                                break;
                            case "RedActivity":
                                RedActivity = ch.Value;
                                break;
                            case "BlueActivity":
                                BlueActivity = ch.Value;
                                break;
                            case "ResetBlueActivity":
                                ResetBlueActivity = ch.Value;
                                break;
                            case "ResetRedActivity":
                                ResetRedActivity = ch.Value;
                                break;
                            case "BlueBonus":
                                BlueBonus = ch.Value;
                                break;
                            case "RedBonus":
                                RedBonus = ch.Value;
                                break;
                            case "BlueDeath":
                                BlueDeath = ch.Value;
                                break;
                            case "RedDeath":
                                RedDeath = ch.Value;
                                break;
                            case "StopBlueDeath":
                                StopBlueDeath = ch.Value;
                                break;
                            case "StopRedDeath":
                                StopRedDeath = ch.Value;
                                break;
                        }
                    }
                    // Получаем текст в текущем узле
                    // MessageBox.Show(table.InnerText);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ошибка чтения файла параметров. Будут использованы клавиши управления по умолчанию.");
                MessageBox.Show(ex.Message);
            }
            #endregion            
        }

        void ItemPopup_Click_BlockText(object sender, RoutedEventArgs e)
        {//Установка\Снятие блокировки редактирования текста
            RedName.IsReadOnly = ((MenuItem)e.Source).IsChecked;
            BlueName.IsReadOnly = ((MenuItem)e.Source).IsChecked;
            Weight.IsReadOnly = ((MenuItem)e.Source).IsChecked;
        }

        void ItemPopup_Click_Time(object sender, RoutedEventArgs e)
        {//Вызов формы установки времени
            SetTime SetTimeWin = new SetTime();
            SetTimeWin.MinutesSlider.Value = ResetMinutes;
            SetTimeWin.SecondsSlider.Value = ResetSeconds;
            if (SetTimeWin.ShowDialog() == true) 
            {
                ResetMinutes = (int)SetTimeWin.MinutesSlider.Value;
                ResetSeconds = (int)SetTimeWin.SecondsSlider.Value;
                _timer.Stop();
                ResetTime();
            };
        }

        void ItemPopup_Click_Param(object sender, RoutedEventArgs e)
        {//Вызов формы установки параметров
            Setting SetWin = new Setting();
            SetWin.RunningLine.Text = RunningLine.Text;
            foreach (var item in Enum.GetValues(typeof(Key)))
            {
                SetWin.RedPointsCombo.Items.Add(item);
                SetWin.BluePointsCombo.Items.Add(item);
                SetWin.ResetRedPointsCombo.Items.Add(item);
                SetWin.ResetBluePointsCombo.Items.Add(item);
                SetWin.RedWarningCombo.Items.Add(item);
                SetWin.BlueWarningCombo.Items.Add(item);
                SetWin.ResetRedWarningCombo.Items.Add(item);
                SetWin.ResetBlueWarningCombo.Items.Add(item);
                SetWin.RedActivityCombo.Items.Add(item);
                SetWin.BlueActivityCombo.Items.Add(item);
                SetWin.ResetRedActivityCombo.Items.Add(item);
                SetWin.ResetBlueActivityCombo.Items.Add(item);
                SetWin.StartPauseTimeCombo.Items.Add(item);
                SetWin.ResetTimeCombo.Items.Add(item);
                SetWin.StartPauseTechTimeRedCombo.Items.Add(item);
                SetWin.ResetTechTimeRedCombo.Items.Add(item);
                SetWin.StartPauseTechTimeBlueCombo.Items.Add(item);
                SetWin.ResetTechTimeBlueCombo.Items.Add(item);
                SetWin.BonusBlueCombo.Items.Add(item);
                SetWin.BonusRedCombo.Items.Add(item);
                SetWin.StartPauseBlueDeathTimeCombo.Items.Add(item);
                SetWin.StartPauseRedDeathTimeCombo.Items.Add(item);
                SetWin.ResetBlueDeathTimeCombo.Items.Add(item);
                SetWin.ResetRedDeathTimeCombo.Items.Add(item);

                if (RedPoints == item.ToString()) SetWin.RedPointsCombo.SelectedItem = item;
                if (BluePoints == item.ToString()) SetWin.BluePointsCombo.SelectedItem = item;
                if (ResetRedPoints == item.ToString()) SetWin.ResetRedPointsCombo.SelectedItem = item;
                if (ResetBluePoints == item.ToString()) SetWin.ResetBluePointsCombo.SelectedItem = item;
                if (RedWarning == item.ToString()) SetWin.RedWarningCombo.SelectedItem = item;
                if (BlueWarning == item.ToString()) SetWin.BlueWarningCombo.SelectedItem = item;
                if ( ResetRedWarning == item.ToString()) SetWin.ResetRedWarningCombo.SelectedItem = item;
                if ( ResetBlueWarning == item.ToString()) SetWin.ResetBlueWarningCombo.SelectedItem = item;
                if ( RedActivity== item.ToString()) SetWin.RedActivityCombo.SelectedItem = item;
                if ( BlueActivity== item.ToString()) SetWin.BlueActivityCombo.SelectedItem = item;
                if ( ResetRedActivity== item.ToString()) SetWin.ResetRedActivityCombo.SelectedItem = item;
                if ( ResetBlueActivity == item.ToString()) SetWin.ResetBlueActivityCombo.SelectedItem = item;
                if ( StartPauseTheTimer== item.ToString()) SetWin.StartPauseTimeCombo.SelectedItem = item;
                if ( ResumeTimer== item.ToString()) SetWin.ResetTimeCombo.SelectedItem = item;
                if ( StartPauseTheTimerRedDoctor== item.ToString()) SetWin.StartPauseTechTimeRedCombo.SelectedItem = item;
                if ( ResetTheTimerRedDoctor== item.ToString()) SetWin.ResetTechTimeRedCombo.SelectedItem = item;
                if ( StartPauseTheTimerBlueDoctor== item.ToString()) SetWin.StartPauseTechTimeBlueCombo.SelectedItem = item;
                if (ResetTheTimerBlueDoctor == item.ToString()) SetWin.ResetTechTimeBlueCombo.SelectedItem = item;
                if (BlueBonus == item.ToString()) SetWin.BonusBlueCombo.SelectedItem = item;
                if (RedBonus == item.ToString()) SetWin.BonusRedCombo.SelectedItem = item;

                if (RedDeath == item.ToString()) SetWin.StartPauseRedDeathTimeCombo.SelectedItem = item;
                if (BlueDeath == item.ToString()) SetWin.StartPauseBlueDeathTimeCombo.SelectedItem = item;
                if (StopRedDeath == item.ToString()) SetWin.ResetRedDeathTimeCombo.SelectedItem = item;
                if (StopBlueDeath == item.ToString()) SetWin.ResetBlueDeathTimeCombo.SelectedItem = item;
            }
            if (SetWin.ShowDialog() == true)
            {
                ReadXML();
                Animate();
                runningLineRestart = true;
            } //Чтение бегущей строки и клавиш из XML. Запуск анимации.
        }

        #region Сброс таймеров
            private void ResetTime()
            {
                Minutes = ResetMinutes;
                Seconds = ResetSeconds;
                string tmp = "0" + Seconds.ToString();
                Time.Text = Minutes + ":" + tmp.Substring(tmp.Length - 2, 2);
                PauseTime.Visibility = System.Windows.Visibility.Hidden;
                PlayTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }

            private void ResetTimeRedTech()
            {
                RedTechMinutes = 0;
                RedTechSeconds = 0;
                _timer_RedDoctor.Stop();
                RedTechTime.Text = "0:00";
                PauseRedTechTime.Visibility = System.Windows.Visibility.Hidden;
                PlayRedTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }

            private void ResetTimeBlueTech()
            {
                BlueTechMinutes = 0;
                BlueTechSeconds = 0;
                _timer_BlueDoctor.Stop();
                BlueTechTime.Text = "0:00";
                PauseBlueTechTime.Visibility = System.Windows.Visibility.Hidden;
                PlayBlueTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }

            private void ResetTimeBlueDeath()
            {
                BlueDeathMinutes = 0;
                BlueDeathSeconds = 0;
                _timer_BlueDeath.Stop();
                BlueTimeDeath.Text = "0:00";
            }

            private void ResetTimeRedDeath()
            {
                RedDeathMinutes = 0;
                RedDeathSeconds = 0;
                _timer_RedDeath.Stop();
                RedTimeDeath.Text = "0:00";
            }     
        #endregion

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

        private void GoKeyDown(object sender, KeyEventArgs e)
        {
            #region включение/выключение полноэкранного режима
            if (e.Key == Key.F11) 
            {//включение\выключение полноэкранного режима
                fullscreen();
            }
            #endregion
            #region обработка событий управления
                    if (e.Key == Key.Escape)
                    {
                        pressKeyLock = false;
                        Time.Focus();

                        allDeath = false;
                        BlueDeathTimerStop();
                        RedDeathTimerStop();
                        

                            //if (PauseBlueTimeDeath.Visibility == System.Windows.Visibility.Visible)
                            //{//проверяем не на паузе ли синий таймер удушения, если да, то сбрасыаем и убираем с экрана
                            //    BlueDeathTimerStop();
                            //}
                            //else
                            //{//, а если нет, то ставим его на паузу
                            //    blueDeathTimerVar = deathTimerEnum.Pause;
                            //    _timer_BlueDeath.Stop();
                            //    PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Visible;                            
                            //}

                            //if (PauseRedTimeDeath.Visibility == System.Windows.Visibility.Visible)
                            //{//проверяем не на паузе ли красный таймер удушения, если да, то сбрасыаем и убираем с экрана
                            //    RedDeathTimerStop();
                            //}
                            //else
                            //{//, а если нет, то ставим его на паузу
                            //    redDeathTimerVar = deathTimerEnum.Pause;
                            //    _timer_RedDeath.Stop();
                            //    PauseRedTimeDeath.Visibility = System.Windows.Visibility.Visible;
                            //}

                    }
                    //################################## БЛОКИРОВАНИЕ КЛАВИШ УПРАВЛЕНИЯ, В СЛУЧАЕ РЕДАКТИРОВАНИЯ ПОЛЕЙ УЧАСТНИКОВ
                    if (pressKeyLock) return;
                    //##################################
                    #region + 1 Очко красному участнику
                            if (e.Key.ToString() == RedPoints) 
                            {
                                RedPointsPlus();
                                return;
                            };
                    #endregion
                    #region + 1 Очко синему участнику
                            if (e.Key.ToString() == BluePoints)
                            {
                                BluePointsPlus();
                                return;
                            };
                    #endregion
                    #region Сброс очков синего участника
                            if (e.Key.ToString() == ResetBluePoints)
                            {
                                BluePointsReset();
                                return;
                            };
                    #endregion
                    #region Сброс очков красного участника
                        if (e.Key.ToString() == ResetRedPoints)
                        {
                            RedPointsReset();
                            return;
                        };
                    #endregion
                    #region + 1 предупреждение красному участнику
                            if (e.Key.ToString() == RedWarning)
                            {
                                RedWarningPlus();
                                return;  
                            };
                    #endregion
                    #region + 1 предупреждение синему участнику
                            if (e.Key.ToString() == BlueWarning)
                            {
                                BlueWarningPlus();
                                return;  
                            };
                    #endregion
                    #region сброс предупреждений синего участника
                            if (e.Key.ToString() == ResetBlueWarning)
                            {
                                BlueWarningReset();
                                return;
                            };
                    #endregion
                    #region сброс предупреждений красного участника
                            if (e.Key.ToString() == ResetRedWarning)
                            {
                                RedWarningReset();
                                return;
                            };
                    #endregion
                    #region + 1 активность красному участнику
                            if (e.Key.ToString() == RedActivity)
                            {
                                RedActivityPlus();
                                return;
                            };
                    #endregion
                    #region + 1 активность синему участнику
                            if (e.Key.ToString() == BlueActivity)
                            {
                                BlueActivityPlus();
                                return;
                            };
                    #endregion
                    #region сброс активностей синего участника
                            if (e.Key.ToString() == ResetBlueActivity)
                            {
                                BlueActivityReset();
                                return;
                            };
                    #endregion
                    #region сброс активностей крастного участника
                            if (e.Key.ToString() == ResetRedActivity)
                            {
                                RedActivityReset();
                                return;
                            };
                            #endregion
                    #region Старт и пауза общего времени
                            if (e.Key.ToString() == StartPauseTheTimer)
                            {
                                PlayTime();
                                return;
                            };
                    #endregion
                    #region Сброс таймера общего времени
                            if (e.Key.ToString() == ResumeTimer)
                            {
                                StopTime();
                                return;
                            };
                    #endregion
                    #region Старт и пауза времени доктора красного участника
                            if (e.Key.ToString() == StartPauseTheTimerRedDoctor)
                            {
                                PlayRedTechTime();
                                return;
                            };
                    #endregion
                    #region Сброс времени доктора красного участника
                            if (e.Key.ToString() == ResetTheTimerRedDoctor)
                            {
                                ResetTimeRedTech();
                                return;
                            };
                    #endregion
                    #region Старт и пауза времени доктора синего участника
                            if (e.Key.ToString() == StartPauseTheTimerBlueDoctor)
                            {
                                PlayBlueTechTime();
                                return;
                            };                    
                    #endregion
                    #region Сброс времени доктора синего участника
                            if (e.Key.ToString() == ResetTheTimerBlueDoctor)
                            {
                                ResetTimeBlueTech();
                                return;
                            };		 
	                #endregion
                    #region + Bonus синему участнику
                            if (e.Key.ToString() == BlueBonus)
                            {
                                BlueBonusMethod();
                                return;
                            };
                    #endregion
                    #region + Bonus красному участнику
                            if (e.Key.ToString() == RedBonus)
                            {
                                RedBonusMethod();
                                return;
                            };
                    #endregion
                    #region Болевое удушение красного участника
                            if (e.Key.ToString() == RedDeath)
                            {
                                RedDeathWindow();
                                return;
                            };
                    #endregion
                    #region Болевое удушение синего участника
                            if (e.Key.ToString() == BlueDeath)
                            {
                                BlueDeathWindow();
                                return;
                            };
                            #endregion
                    #region Стоп - Болевое удушение красного участника
                            if (e.Key.ToString() == StopRedDeath)
                            {
                                //-----------------------------------------------------------------------------
                                if (RedTimeDeathGrid.Visibility == System.Windows.Visibility.Visible) 
                                {
                                    RedDeathTimerStop();
                                }
                                return;
                            };
                    #endregion
                    #region Стоп - Болевое удушение синего участника
                            if (e.Key.ToString() == StopBlueDeath)
                            {
                                //-----------------------------------------------------------------------------
                                if (BlueTimeDeathGrid.Visibility == System.Windows.Visibility.Visible) 
                                {
                                    BlueDeathTimerStop();
                                }
                                return;
                            };
                     #endregion
            #endregion
        }

        private void RedBonusMethod()
        {
            if (RedScore.Text.Substring(0, 1) != "+")
            {
                RedScore.Text = "+" + RedScore.Text;
            }
            else
            {
                RedScore.Text = RedScore.Text.Substring(1, RedScore.Text.Length - 1);
            }
            SetRedScoreFontSize();
        }

        private void BlueBonusMethod()
        {
            if (BlueScore.Text.Substring(0, 1) != "+")
            {
                BlueScore.Text = "+" + BlueScore.Text;
            }
            else
            {
                BlueScore.Text = BlueScore.Text.Substring(1, BlueScore.Text.Length - 1);
            }
            SetBlueScoreFontSize();
        }

        private void RedPointsReset()
        {
            RedScore.Text = "0";
            RedScore.FontSize = 80;
        }

        private void BluePointsReset()
        {
            BlueScore.Text = "0";
            BlueScore.FontSize = 80;
        }

        private void BlueDeathWindow()
        {
            if (_timer_BlueDeath.IsEnabled)
            {
                blueDeathTimerVar = deathTimerEnum.Pause;
                _timer_BlueDeath.Stop();
                PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Visible;
                PlayBlueTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                BlueDeathTimerStart();
            };

            ////-----------------------------------------------------------------------------
            //if (!_timer_RedDeath.IsEnabled)
            //{//если красный таймер удушения выключен, то выводим синий таймер удушения
            //    if (_timer_BlueDeath.IsEnabled)
            //    {
            //        blueDeathTimerVar = deathTimerEnum.Pause;
            //        _timer_BlueDeath.Stop();
            //        PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Visible;
            //        PlayBlueTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            //    }
            //    else
            //    {
            //        BlueDeathTimerStart();
            //        if (PauseRedTimeDeath.Visibility == System.Windows.Visibility.Visible)
            //        {//если красный таймер удушения включен, тогда сбрасываем его и убираем с экрана 
            //            RedDeathTimerStop();
            //        }
            //    }
            //}
            //else
            //{//иначе, ставим его на паузу
            //    blueDeathTimerVar = deathTimerEnum.Pause;
            //    _timer_RedDeath.Stop();
            //    PauseRedTimeDeath.Visibility = System.Windows.Visibility.Visible;
            //    PlayBlueTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            //}
        }

        private void RedDeathWindow()
        {
            if (_timer_RedDeath.IsEnabled)
            {
                redDeathTimerVar = deathTimerEnum.Pause;
                _timer_RedDeath.Stop();
                PauseRedTimeDeath.Visibility = System.Windows.Visibility.Visible;
                PlayRedTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                RedDeathTimerStart();
            };
            ////-----------------------------------------------------------------------------
            //if (!_timer_BlueDeath.IsEnabled)
            //{//если синий таймер удушения выключен, то выводим красный таймер удушения
            //    if (_timer_RedDeath.IsEnabled)
            //    {
            //        redDeathTimerVar = deathTimerEnum.Pause;
            //        _timer_RedDeath.Stop();
            //        PauseRedTimeDeath.Visibility = System.Windows.Visibility.Visible;
            //        PlayRedTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            //    }
            //    else
            //    {
            //        RedDeathTimerStart();
            //        if (PauseBlueTimeDeath.Visibility == System.Windows.Visibility.Visible)
            //        {//если синий таймер удушения включен, тогда сбрасываем его и убираем с экрана 
            //            BlueDeathTimerStop();
            //        }
            //    }
            //}
            //else
            //{//иначе, ставим его на паузу
            //    redDeathTimerVar = deathTimerEnum.Pause;
            //    _timer_BlueDeath.Stop();
            //    PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Visible;
            //    PlayRedTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            //}
        }

        private void RedPointsPlus()
        {
            string tmpS = RedScore.Text;
            string Bonus = "";
            if (tmpS.Substring(0, 1) == "+")
            {
                Bonus = "+";
                tmpS = RedScore.Text.Substring(1, RedScore.Text.Length - 1);
            }
            int i = Convert.ToInt32(tmpS);
            if (i < maxScore)
            {
                i++;
            }
            RedScore.Text = Bonus + i.ToString();
            SetRedScoreFontSize();
        }

        private void BluePointsPlus()
        {
            string tmpS = BlueScore.Text;
            string Bonus = "";
            if (tmpS.Substring(0, 1) == "+")
            {
                Bonus = "+";
                tmpS = BlueScore.Text.Substring(1, BlueScore.Text.Length - 1);
            }
            int i = Convert.ToInt32(tmpS);
            if (i < maxScore)
            {
                i++;
            }
            BlueScore.Text = Bonus + i.ToString();
            SetBlueScoreFontSize();
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

        private void RedDeathTimerStart()
        {
            redDeathTimerVar = deathTimerEnum.Start;
            _timer_RedDeath.Start();
            PauseRedTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            PlayRedTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            if (RedTimeDeathGrid.Visibility == System.Windows.Visibility.Hidden)
            {
                RedTimeDeathGrid.Visibility = System.Windows.Visibility.Visible;
                RedDeathAnimation.From = -180;
                RedDeathAnimation.To = 64;
                RedDeathAnim.Begin(this, true);
            }
        }

        private void RedDeathTimerStop()
        {
            redDeathTimerVar = deathTimerEnum.Stop;
            _timer_RedDeath.Stop();
            PauseRedTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            RedDeathAnimation.From = 64;
            RedDeathAnimation.To = -180;
            RedDeathAnim.Begin(this, true);
            ResetTimeRedDeath();

            if (BlueTimeDeathGrid.Visibility == System.Windows.Visibility.Hidden)
            {
                allDeath = false;
            }
        }

        private void BlueDeathTimerStart()
        {
            blueDeathTimerVar = deathTimerEnum.Start;
            _timer_BlueDeath.Start();
            PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            PlayBlueTimeDeathImage.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            if (BlueTimeDeathGrid.Visibility == System.Windows.Visibility.Hidden)
            {
                BlueTimeDeathGrid.Visibility = System.Windows.Visibility.Visible;
                BlueDeathAnimation.From = 628;
                BlueDeathAnimation.To = 384;
                BlueDeathAnim.Begin(this, true);
            }
        }

        private void BlueDeathTimerStop()
        {
            blueDeathTimerVar = deathTimerEnum.Stop;
            _timer_BlueDeath.Stop();
            PauseBlueTimeDeath.Visibility = System.Windows.Visibility.Hidden;
            BlueDeathAnimation.From = 384;
            BlueDeathAnimation.To = 628;
            BlueDeathAnim.Begin(this, true);
            ResetTimeBlueDeath();

            if (RedTimeDeathGrid.Visibility == System.Windows.Visibility.Hidden)
            {
                allDeath = false;
            }
        }

        private void RedDeathAnimation_Completed(object sender, EventArgs e)
        {
            if (!_timer_RedDeath.IsEnabled) RedTimeDeathGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void BlueDeathAnimation_Completed(object sender, EventArgs e)
        {
            if (!_timer_BlueDeath.IsEnabled) BlueTimeDeathGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void imageDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (!hallWindowOpen) {
                var hallWindow = new HallWindow();
                hallWindow.Owner = this;
                hallWindow.Show();
                hallWindowOpen = true;            
            }
        }

        private void imageRedScorePlusImage_Click(object sender, RoutedEventArgs e)
        {
            RedPointsPlus();
        }

        private void imageBlueScorePlusImage_Click(object sender, RoutedEventArgs e)
        {
            BluePointsPlus();
        }

        private void imageRedScoreResetImage_Click(object sender, RoutedEventArgs e)
        {
            RedPointsReset();
        }

        private void imageBlueScoreResetImage_Click(object sender, RoutedEventArgs e)
        {
            BluePointsReset();
        }

        private void imageRedScoreActivityImage_Click(object sender, RoutedEventArgs e)
        {
            RedBonusMethod();
        }

        private void imageBlueScoreActivityImage_Click(object sender, RoutedEventArgs e)
        {
            BlueBonusMethod();
        }

        private void imageRedActivityPlusImage_Click(object sender, RoutedEventArgs e)
        {
            RedActivityPlus();
        }

        private void imageRedActivityResetImage_Click(object sender, RoutedEventArgs e)
        {
            RedActivityReset();
        }

        private void imageBlueActivityPlusImage_Click(object sender, RoutedEventArgs e)
        {
            BlueActivityPlus();
        }

        private void imageRedWarningPlusImage_Click(object sender, RoutedEventArgs e)
        {
            RedWarningPlus();
        }

        private void RedWarningPlus()
        {
            int i = Convert.ToInt32(RedWarningText.Text);
            i++;
            RedWarningText.Text = i.ToString();
        }

        private void imageRedWarningResetImage_Click(object sender, RoutedEventArgs e)
        {
            RedWarningReset();
        }

        private void RedWarningReset()
        {
            RedWarningText.Text = "0";
        }

        private void imageBlueWarningPlusImage_Click(object sender, RoutedEventArgs e)
        {
            BlueWarningPlus();
        }

        private void BlueWarningPlus()
        {
            int i = Convert.ToInt32(BlueWarningText.Text);
            i++;
            BlueWarningText.Text = i.ToString();
        }

        private void imageBlueWarningResetImage_Click(object sender, RoutedEventArgs e)
        {
            BlueWarningReset();
        }

        private void BlueWarningReset()
        {
            BlueWarningText.Text = "0";
        }

        private void BlueActivityPlus()
        {
            int i = Convert.ToInt32(BlueActivityText.Text);
            i++;
            BlueActivityText.Text = i.ToString();
        }

        private void imageBlueActivityResetImage_Click(object sender, RoutedEventArgs e)
        {
            BlueActivityReset();
        }

        private void BlueActivityReset()
        {
            BlueActivityText.Text = "0";
        }

        private void RedActivityReset()
        {
            RedActivityText.Text = "0";
        }
        
        private void RedActivityPlus()
        {
            int i = Convert.ToInt32(RedActivityText.Text);
            i++;
            RedActivityText.Text = i.ToString();
        }

        private void imageRedMemberChangeImage_Click(object sender, RoutedEventArgs e)
        {
            RedSelectNameMember();
        }

        private void imageBlueMemberChangeImage_Click(object sender, RoutedEventArgs e)
        {
            BlueSelectNameMember();
        }

        private void imagePlayTimeImage_Click(object sender, RoutedEventArgs e)
        {
            PlayTime();
        }

        private void imageRedNameResetImage_Click(object sender, RoutedEventArgs e)
        {
            RedName.Text = "";
            textBoxTeamRed.Text = "";
        }

        private void imageBlueNameResetImage_Click(object sender, RoutedEventArgs e)
        {
            BlueName.Text = "";
            textBoxTeamBlue.Text = "";
        }

        private void PlayTime()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                PauseTime.Visibility = System.Windows.Visibility.Visible;
                PlayTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                if (Seconds <= 0 && Minutes <= 0) ResetTime();
                _timer.Start();
                PauseTime.Visibility = System.Windows.Visibility.Hidden;
                PlayTimeImage.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            }
        }

        private void imageStopTimeImage_Click(object sender, RoutedEventArgs e)
        {
            StopTime();
        }

        private void StopTime()
        {
            ResetTime();
            string tmp = "0" + Seconds.ToString();
            Time.Text = Minutes + ":" + tmp.Substring(tmp.Length - 2, 2);
            _timer.Stop();
        }

        private void imagePlayRedTechTimeImage_Click(object sender, RoutedEventArgs e)
        {
            PlayRedTechTime();
        }

        private void PlayRedTechTime()
        {
            if (_timer_RedDoctor.IsEnabled)
            {
                _timer_RedDoctor.Stop();
                PauseRedTechTime.Visibility = System.Windows.Visibility.Visible;
                PlayRedTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                _timer_RedDoctor.Start();
                PauseRedTechTime.Visibility = System.Windows.Visibility.Hidden;
                PlayRedTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            }
        }

        private void imageStopRedTechTimeImage_Click(object sender, RoutedEventArgs e)
        {
            ResetTimeRedTech();
        }

        private void imagePlayBlueTechTimeImage_Click(object sender, RoutedEventArgs e)
        {
            PlayBlueTechTime();
        }

        private void PlayBlueTechTime()
        {
            if (_timer_BlueDoctor.IsEnabled)
            {
                _timer_BlueDoctor.Stop();
                PauseBlueTechTime.Visibility = System.Windows.Visibility.Visible;
                PlayBlueTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                _timer_BlueDoctor.Start();
                PauseBlueTechTime.Visibility = System.Windows.Visibility.Hidden;
                PlayBlueTechTimeImage.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            }
        }

        private void imageStopBlueTechTimeImage_Click(object sender, RoutedEventArgs e)
        {
            ResetTimeBlueTech();
        }

        private void imagePlayRedTimeDeathImage_Click(object sender, RoutedEventArgs e)
        {
            RedDeathWindow();
        }

        private void imageStopRedTimeDeathImage_Click(object sender, RoutedEventArgs e)
        {
            RedDeathTimerStop();
        }

        private void imagePlayBlueTimeDeathImage_Click(object sender, RoutedEventArgs e)
        {
            BlueDeathWindow();
        }

        private void imageStopBlueTimeDeathImage_Click(object sender, RoutedEventArgs e)
        {
            BlueDeathTimerStop();
        }

        private void imageRedPain_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RedDeathWindow();
        }

        private void imageBluePain_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            BlueDeathWindow();
        }

        private void imagePain_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            if (!allDeath)
            {
                if (BlueTimeDeathGrid.Visibility == System.Windows.Visibility.Visible || RedTimeDeathGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    allDeath = false;
                    RedDeathTimerStop();
                    BlueDeathTimerStop();
                }
                else
                {
                    allDeath = true;
                    BlueDeathTimerStart();
                    RedDeathTimerStart();
                }
            }
            else
            {
                allDeath = false;
                RedDeathTimerStop();
                BlueDeathTimerStop();
            };
        }

        private void imageClear_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Сбросить счётчики и очистить поля?", "Подтвердите действие", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                StopTime();
                ResetTimeRedTech();
                ResetTimeBlueTech();
                RedDeathTimerStop();
                BlueDeathTimerStop();
                RedScore.Text = "0";
                BlueScore.Text = "0";
                RedActivityText.Text = "0";
                BlueActivityText.Text = "0";
                RedWarningText.Text = "0";
                BlueWarningText.Text = "0";

                RedName.Text = "";
                BlueName.Text = "";
                textBoxTeamRed.Text = "";
                textBoxTeamBlue.Text = "";
            }
        }
        
        private void RedName_KeyDown(object sender, KeyEventArgs e)
        {
            pressKeyLock = true;
        }

        private void RedName_KeyUp(object sender, KeyEventArgs e)
        {
            pressKeyLock = false;
        }

        private void BlueName_KeyDown(object sender, KeyEventArgs e)
        {
            pressKeyLock = true;
        }

        private void BlueName_KeyUp(object sender, KeyEventArgs e)
        {
            pressKeyLock = false;
        }

        private void textBoxTeamRed_KeyDown(object sender, KeyEventArgs e)
        {
            pressKeyLock = true;
        }

        private void textBoxTeamRed_KeyUp(object sender, KeyEventArgs e)
        {
            pressKeyLock = false;
        }

        private void textBoxTeamBlue_KeyDown(object sender, KeyEventArgs e)
        {
            pressKeyLock = true;
        }

        private void textBoxTeamBlue_KeyUp(object sender, KeyEventArgs e)
        {
            pressKeyLock = false;
        }

        private void Weight_KeyDown(object sender, KeyEventArgs e)
        {
            pressKeyLock = true;
        }

        private void Weight_KeyUp(object sender, KeyEventArgs e)
        {
            pressKeyLock = false;
        }

        private void imageExcel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ExcelWindow ExcelWin = new ExcelWindow();
            if (ExcelWin.ShowDialog() == true)
            {

            }
        }

        private void RedName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RedSelectNameMember();
        }

        private void RedSelectNameMember()
        {
            SelNameWindow selNameWin = new SelNameWindow();
            selNameWin.Top = Application.Current.MainWindow.Top + Mouse.GetPosition(Application.Current.MainWindow).Y;
            selNameWin.Left = Application.Current.MainWindow.Left + Mouse.GetPosition(Application.Current.MainWindow).X;
            if (selNameWin.ShowDialog() == true)
            {
                RedName.Text = selNameWin.name;
                textBoxTeamRed.Text = selNameWin.team;
            }
            Time.Focus();
        }

        private void BlueName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BlueSelectNameMember();
        }

        private void BlueSelectNameMember()
        {
            SelNameWindow selNameWin = new SelNameWindow();
            selNameWin.Top = Application.Current.MainWindow.Top + Mouse.GetPosition(Application.Current.MainWindow).Y;
            selNameWin.Left = Application.Current.MainWindow.Left + Mouse.GetPosition(Application.Current.MainWindow).X;
            if (selNameWin.ShowDialog() == true)
            {
                BlueName.Text = selNameWin.name;
                textBoxTeamBlue.Text = selNameWin.team;
            }
            Time.Focus();
        }

    }
}
