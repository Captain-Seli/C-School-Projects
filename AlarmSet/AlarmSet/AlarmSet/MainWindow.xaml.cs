// CSE 483: Windows Programming in C#
// Syracuse University, Spring 2021
// Digital Clock and Application Final Project
// Remote Application portion
// Authors Jonathan Williams and Joe Zoll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlarmSet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model _model;
        public MainWindow()
        {
            InitializeComponent();
            _model = new Model();
            this.DataContext = _model;
            this.ResizeMode = ResizeMode.NoResize;
        }

        // Set the time to whatever time you want
        private void setTimeButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonSelect = "Set Time";
            _model.sendData(buttonSelect);
        }

        private void nowTimeButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonSelect = "Current Time";
            _model.sendData(buttonSelect);
        }

        private void setAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonSelect = "Set Alarm";
            _model.sendData(buttonSelect);
        }

    }
}
