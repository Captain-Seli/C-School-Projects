// CSE 483: Windows Programming in C#
// Syracuse University, Spring 2021
// Digital Clock and Application Final Project
// Digital Clock portion
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

namespace DigitalClock
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

            this.ResizeMode = ResizeMode.NoResize;
            _model = new Model();
            this.DataContext = _model;
            _model.InitModel();

            // create the observable collection for the LED's
            SevenSegmentLED.ItemsSource = _model.LED_Collection;

        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) 
        {
            _model.Cleanup();
        }
    }
}
