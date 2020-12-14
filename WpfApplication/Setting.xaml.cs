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
using System.Xml.Linq;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Сохранить изменения?", "Подтвердите действие", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                XElement Config = XElement.Load(@"custom.xml");
                #region коммент
                //this.Top = (double)_appconfig.Element("MainWindow").Attribute("top");
                //MessageBox.Show((string)Config.Element("Text").Value);
                #endregion
                Config.Element("Text").Value = RunningLine.Text;
                Config.Element("RedPoints").Value = RedPointsCombo.Text;
                Config.Element("BluePoints").Value = BluePointsCombo.Text;
                Config.Element("ResetRedPoints").Value = ResetRedPointsCombo.Text;
                Config.Element("ResetBluePoints").Value = ResetBluePointsCombo.Text;
                Config.Element("RedWarning").Value = RedWarningCombo.Text;
                Config.Element("BlueWarning").Value = BlueWarningCombo.Text;
                Config.Element("ResetRedWarning").Value = ResetRedWarningCombo.Text;
                Config.Element("ResetBlueWarning").Value = ResetBlueWarningCombo.Text;
                Config.Element("RedActivity").Value = RedActivityCombo.Text;
                Config.Element("BlueActivity").Value = BlueActivityCombo.Text;
                Config.Element("ResetRedActivity").Value = ResetRedActivityCombo.Text;
                Config.Element("ResetBlueActivity").Value = ResetBlueActivityCombo.Text;
                Config.Element("StartPauseTheTimer").Value = StartPauseTimeCombo.Text;
                Config.Element("ResumeTimer").Value = ResetTimeCombo.Text;
                Config.Element("StartPauseTheTimerRedDoctor").Value = StartPauseTechTimeRedCombo.Text;
                Config.Element("ResetTheTimerRedDoctor").Value = ResetTechTimeRedCombo.Text;
                Config.Element("StartPauseTheTimerBlueDoctor").Value = StartPauseTechTimeBlueCombo.Text;
                Config.Element("ResetTheTimerBlueDoctor").Value = ResetTechTimeBlueCombo.Text;
                Config.Element("RedBonus").Value = BonusRedCombo.Text;
                Config.Element("BlueBonus").Value = BonusBlueCombo.Text;
                Config.Element("BlueDeath").Value = StartPauseBlueDeathTimeCombo.Text;
                Config.Element("RedDeath").Value = StartPauseRedDeathTimeCombo.Text;
                Config.Element("StopBlueDeath").Value = ResetBlueDeathTimeCombo.Text;
                Config.Element("StopRedDeath").Value = ResetRedDeathTimeCombo.Text;                

                Config.Save(@"custom.xml");
                this.DialogResult = true;
            }
        }
    }
}
