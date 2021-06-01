using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Sockets
using System.Net.Sockets;
using System.Net;
using System.Threading;
// INotifyPropertyChanged
using System.ComponentModel;


namespace ExampleUdpClient
{
    class Model : INotifyPropertyChanged
    {
        // some data that keeps track of ports and addresses
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

        private String _myFriendBox;
        public String MyFriendBox
        {
            get { return _myFriendBox; }
            set
            {
                _myFriendBox = value;
                OnPropertyChanged("MyFriendBox");
            }
        }

        private String _statusBox;
        public String StatusBox
        {
            get { return _statusBox; }
            set
            {
                _statusBox = value;
                OnPropertyChanged("StatusBox");
            }
        }

        private String _loopBox;
        public String LoopBox
        {
            get { return _loopBox; }
            set
            {
                _loopBox = value;
                OnPropertyChanged("LoopBox");
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


        public void SendMessage()
        {
            IPEndPoint remoteHost = new IPEndPoint(IPAddress.Parse(_remoteIPAddress), (int)_remotePort);
            Byte[] sendBytes = Encoding.ASCII.GetBytes(MyFriendBox);

            try
            {
                _dataSocket.Send(sendBytes, sendBytes.Length, remoteHost);
            }
            catch (SocketException ex)
            {
                StatusBox = StatusBox + DateTime.Now + ":" + ex.ToString();
                return;
            }
            // Try catch to recieve the echo from the server
            try
            {
                // recieved bytes
                Byte[] recieveBytes = _dataSocket.Receive(ref remoteHost);
                // Update Status Box with Loop update
                StatusBox += DateTime.Now + ":" + "Looped Message Recieved" + "\n";
                // Display the DateTime and the echo in the loop box window
                LoopBox += DateTime.Now + ": " + System.Text.Encoding.Default.GetString(recieveBytes) + "\n";
            }
            // Catch any exceptions
            catch (SocketException ex)
            { 

                StatusBox += DateTime.Now + ":" + ex.ToString();
                return;
            }
        }

    }
}
