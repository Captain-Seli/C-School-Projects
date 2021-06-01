// CSE 483: Windows Programming in C#
// Syracuse University, Spring 2021
// Digital Clock and Remote Application: Final Project
// Digital Clock portion
// Authors Jonathan Williams and Joe Zoll
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Shapes;
// Stopwatch, Debug
using System.Diagnostics;
// WPF Timer
using System.Windows.Threading;
// Threads
using System.Threading;
// Timer.Timer
using System.Timers;
using HighPrecisionTimer;
// Rectangle
using System.Drawing;
// Sockets
using System.Net.Sockets;
using System.Net;
// other stuff in BallPushDemo
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

// Slide 7 of timer powerpoint
//using System.Threading.Timer;

// observable collections
using System.Collections.ObjectModel;

namespace DigitalClock
{
    public partial class Model : INotifyPropertyChanged
    {

        // data that keeps track of ports and addresses
        private static int _localPort = 5000;
        private static string _localIPAddress = "127.0.0.1";
        // Check bool to tell when the 24 hour box is checked
        bool CheckBox;
        // Alarm bool to tell when the Alarm is being sent from the application
        int alarm = 0;

        // DLL Imports
        public static TimeDataDLL.TimeData.StructTimeData _currTime; // Current Time
        public static TimeDataDLL.TimeData.StructTimeData _setTime;  // Set the time
        public static TimeDataDLL.TimeData.StructTimeData _alarm;    // Alarm

        // this is the thread that will run in the background
        // waiting for incomming data
        private Thread _receiveDataThread;

        // this is the UDP socket that will be used to communicate
        // over the network
        UdpClient _dataSocket;

        // Alarm Label to display the alarm
        private string _alarmLabel;
        public string AlarmLabel
        {
            get { return _alarmLabel; }
            set
            {
                _alarmLabel = value;
                OnPropertyChanged("AlarmLabel");
            }
        }

        // AM PM Label
        private string _AmPmLabel;
        public string AmPmLabel
        {
            get { return _AmPmLabel; }
            set
            {
                _AmPmLabel = value;
                OnPropertyChanged("AmPmLebel");
            }
        }
        // Function to recieve data from the Clock App
        private void ReceiveThreadFunction()
        {
            // Create the endpoint
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(_localIPAddress), (int)_localPort);
            while (true)
            {
                try
                {
                    // wait for data
                    // this is a blocking call
                    Byte[] receiveData = _dataSocket.Receive(ref endPoint);
                    // If the byte 3 is 0 the selected button is "Set Time"
                    if (receiveData[3] == 0)
                    {
                        // Set Custom Time
                        _setTime = new TimeDataDLL.TimeData.StructTimeData(receiveData[0], receiveData[1], receiveData[2]);
                        _currTime = _setTime;
                        // Test
                        Console.WriteLine(_setTime.hour);

                    }
                    // If byte 3 is 1, selected button is "Current Time"
                    else if (receiveData[3] == 1)
                    {
                        // Current Time
                        DateTime currTime = DateTime.Now;
                        _currTime = new TimeDataDLL.TimeData.StructTimeData(currTime.Hour, currTime.Minute, currTime.Second);
                        Console.WriteLine(_currTime.hour);

                    }
                    // If byte 5 is a 1, the alarm has been set
                    else if (receiveData[5] == 1)
                    {
                        // Set Alarm
                        _alarm = new TimeDataDLL.TimeData.StructTimeData(receiveData[0], receiveData[1], receiveData[2]);
                        alarm = 1; // Alarm flag for use in timeUpdate()
                        Console.WriteLine("Alarm recieved.");
                    }
                    // If byte 4 is not 0, the 24 hour check is not checked
                    CheckBox = (receiveData[4] != 0);
                }
                catch (SocketException ex)
                {
                    // got here because either the Receive failed, or more
                    // or more likely the socket was destroyed by 
                    // exiting from the JoystickPositionWindow form
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }


        // Observable Collection for LED's
        public ObservableCollection<LED> LED_Collection;

        // From TimerDemo Model.cs
        // Line 33 - Stopwatch is a class that allows us to measure time very accurately
        Stopwatch stopWatch;

        // From TimerDemo Model.cs
        // Line 37-40 - Using one timer, so only one bool is necessary
        bool _netTimerTimerRunning = false;
        // .NET Timer stuff from TimerDemo Model.cs
        uint NETTimerTimerTicks = 0;
        long NETTimerTimerTotalTime = 0;
        long NETTimerTimerPreviousTime;
        // The actual timer
        System.Timers.Timer dotNetTimerTimer;

        // The function that starts the timer
        public void NETTimerTimerStart(bool startStop) 
        {
            if (startStop == true)
            {
                dotNetTimerTimer = new System.Timers.Timer(1000);
                dotNetTimerTimer.Elapsed += new ElapsedEventHandler(NetTimerTimerHandler);
                dotNetTimerTimer.Start(); // starts timer
            }
            else if (_netTimerTimerRunning)
            {
                dotNetTimerTimer.Stop();
            }
        }

        // The timer handler function
        private void NetTimerTimerHandler(object source, ElapsedEventArgs e)
        {
            // Update the time each tick
            timeUpdate();
            // Gets each digit for the LEDs and displays them
            getDigit(_currTime);
        }

        // getDigit gets the digits for the LEDs and displays them
        void getDigit(TimeDataDLL.TimeData.StructTimeData _currTimeInput)
        {   
            // For 24 hour time
            if (CheckBox)
            {
                LED_Collection[0].LEDValue = (UInt32)(_currTimeInput.hour % 100) / 10;
                LED_Collection[1].LEDValue = (UInt32)(_currTimeInput.hour % 10);
                LED_Collection[2].LEDValue = (UInt32)(_currTimeInput.minute % 100) / 10;
                LED_Collection[3].LEDValue = (UInt32)(_currTimeInput.minute % 10);
                LED_Collection[4].LEDValue = (UInt32)(_currTimeInput.second % 100) / 10;
                LED_Collection[5].LEDValue = (UInt32)(_currTimeInput.second % 10);
            }
            // For 12 hour time
            else if (!CheckBox)
            {
                UInt32 x;
                if (_currTimeInput.hour > 12)
                {
                    x = (UInt32)_currTimeInput.hour - 12;
                }

                else if (_currTimeInput.hour <= 12)
                {
                    x = (UInt32)_currTimeInput.hour;
                }

                else
                {
                    // x needs a value, so here we make it 0
                    x = 0;
                }

                // Above else is to prevent the x from having no value
                LED_Collection[0].LEDValue = (UInt32)(x % 100) / 10;
                LED_Collection[1].LEDValue = (UInt32)(x % 10);
                LED_Collection[2].LEDValue = (UInt32)(_currTimeInput.minute % 100) / 10;
                LED_Collection[3].LEDValue = (UInt32)(_currTimeInput.minute % 10);
                LED_Collection[4].LEDValue = (UInt32)(_currTimeInput.second % 100) / 10;
                LED_Collection[5].LEDValue = (UInt32)(_currTimeInput.second % 10);
            }
        }

        // Alarm Update Function
        private void AlarmUpdate(TimeDataDLL.TimeData.StructTimeData setTime, TimeDataDLL.TimeData.StructTimeData AlarmTime)
        {
            if (!CheckBox)
            {
                if (setTime.hour > 12)
                {
                    setTime.hour -= 12;
                }
            }

            try
            {   
                if ((setTime.hour == _alarm.hour) && (setTime.minute == _alarm.minute) && ((setTime.second - _alarm.second <= 5 && setTime.second - _alarm.second >= 0) || (setTime.second == _alarm.second)))
                {   
                    AlarmLabel = "ALARM"; // Update the label when the alarm is reached
                }

                else
                {
                    AlarmLabel = null; // Label is null otherwise
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Time update function - Tracks seconds, sets off alarm
        private void timeUpdate()
        {
            if(_currTime.second >= 59)
            {
                _currTime.minute++;
                _currTime.second = 0;
            }
            else
            {
                _currTime.second++;
            }
            if (_currTime.minute >= 59)
            {
                _currTime.hour++;
                _currTime.minute = 0;
            }
            if (_currTime.hour == 23 && _currTime.minute == 59 && _currTime.second == 59)
            {
                _currTime.second = 0;
                _currTime.minute = 0;
                _currTime.hour = 0;
            }
            // If alarm is received, call AlarmUpdate
            if (alarm == 1)
            {
                AlarmUpdate(_currTime, _alarm);
            }

                
        }

        public Model()
        {
            _dataSocket = new UdpClient(_localPort);

            // start the thread to listen for data from other UDP peer
            ThreadStart threadFunction = new ThreadStart(ReceiveThreadFunction);
            _receiveDataThread = new Thread(threadFunction);
            _receiveDataThread.Start();

          //  _randomNumber = new Random();

            // TimerDemo Model.cs
            // Line 44
            stopWatch = new Stopwatch();
            stopWatch.Start();
            NETTimerTimerStart(true);
        }

        public void Cleanup()
        {
            Console.WriteLine("Aborting...");
            stopWatch.Stop();
            NETTimerTimerStart(false);

            // if we don't close the socket and abort the thread, 
            // the applicatoin will not close properly
            if (_dataSocket != null) _dataSocket.Close();
            if (_receiveDataThread != null) _receiveDataThread.Abort();
        }

        public void InitModel()
        {
            LED_Collection = new ObservableCollection<LED>();


            // Adding to LED observable collection
            for (int i = 0; i < 6; ++i)
            {
                LED_Collection.Add(new LED()
                {
                    LEDLeft = 60 * i,
                    LEDTop = 60,
                    LEDValue = 0,
                });
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
