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
using System.Windows.Threading;
// Sockets
using System.Net.Sockets;
using System.Net;
using System.Threading;
// INotifyPropertyChanged
using System.ComponentModel;




namespace AlarmSet
{
    class Model : INotifyPropertyChanged
    {
        // data that keeps track of ports and addresses
        private static UInt32 _remotePort = 5000;
        private static String _remoteIPAddress = "127.0.0.1";
        // this is the UDP socket that will be used to communicate
        // over the network
        private UdpClient _dataSocket;


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Variables for hours, minutes, seconds, alarm, and check box
        public int _hours;
        public int Hours
        {
            get { return _hours; }
            set
            {
                _hours = value;
                OnPropertyChanged("Hours");
            }
        }

        public int _minutes;
        public int Minutes
        {
            get { return _minutes; }
            set
            {
                _minutes = value;
                OnPropertyChanged("Minutes");
            }
        }

        public int _seconds; 
        public int Seconds
        {
            get { return _seconds; }
            set
            {
                _seconds = value;
                OnPropertyChanged("Seconds");
            }
        }

        public int _alarm = 0;
        public int Alarm
        {
            get { return _alarm; }
            set
            {
                _alarm = value;
                OnPropertyChanged("Alarm");
            }
        }

        public bool _checkBox;
        public bool CheckBox
        {
            get { return _checkBox; }
            set
            {
                _checkBox = value;
                OnPropertyChanged("CheckBox");
            }
        }

        public Model()
        {
            try
            {
                // set up generic UDP socket and bind to local port
                _dataSocket = new UdpClient();

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        // Function that sends the data 
        public void sendData(string button)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
            // allows the leading 0 in the textboxes
            int buttonSelect = 2;
            client.Connect(ep);

            if (button == "Set Time")
            {
                buttonSelect = 0;
            }
            if (button == "Current Time")
            {
                buttonSelect = 1;
            }
            if (button == "Set Alarm")
            {
                _alarm = 1;
                
            }
            // hours = 0, mins = 1, seconds = 2, buttonSelect = 3, CheckBox Bool = 4, alarm  = 5
            Byte[] sendData = { (Byte)_hours, (Byte)_minutes, (Byte)_seconds, (Byte)buttonSelect, (Byte)Convert.ToInt32(CheckBox), (Byte)_alarm };

            try
            {
                // Send actual data to clock
                _dataSocket.Send(sendData, sendData.Length, ep);
                // Send each piece of data separate over the UDP connection
                Console.WriteLine("Packet sent successfully.");
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        } 
        // When check box is checked, display 24 hour clock
        public  bool Upchecked()
        {
            return true;
        }
        
        // When checkbox is unchecked, display 12 hour clock
        public bool uncheck()
        {
            return true;
        }
    }
}