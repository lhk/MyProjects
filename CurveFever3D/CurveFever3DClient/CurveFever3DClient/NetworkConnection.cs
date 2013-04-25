using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace CurveFever3DClient
{
    class NetworkConnection
    {
        Socket socket;
        public enum Status { Disconnected, Connecting, Connected };
        public Status status;
        public string errorMessage;

        public NetworkConnection()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
        }

        public void BeginConnect()
        {
            Thread connectThread = new Thread(new ThreadStart(delegate() 
                {
                    status = Status.Connecting;
                    socket.Connect("178.63.14.80", 3449);
                    status = Status.Connected;
                }));
            

        }

        /*
        
         
         
            // network test
            if (clientSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[4096];
                int bytes = clientSocket.Receive(buffer);
                string[] lines = Encoding.ASCII.GetString(buffer, 0, bytes).Split('\n');

                foreach (var line in lines)
                {
                    string[] elements = line.Trim().Split('=');
                    if (0 < elements.Length && elements.Length <= 2)
                    {
                        string name = elements[0].Trim();

                        if (name == "YourPosition")
                        {
                            string[] coordinates = elements[1].Substring(1, elements[1].Length - 2).Split(':');
                            float x, y, z;
                            if (coordinates.Length == 3 &&
                                float.TryParse(coordinates[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                                float.TryParse(coordinates[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                                float.TryParse(coordinates[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
                            {
                                cam.Position = new Vector3(x, y, z);
                            }
                        }
                    }
                }

                //Console.WriteLine("Received: \"" + Encoding.ASCII.GetString(buffer, 0, bytes) + "\"");
            }
            if (clientSocket.Poll(0, SelectMode.SelectWrite))
            {
                string s = "Pitch=" + (cam.Pitch).ToString("############0.0#############", System.Globalization.CultureInfo.InvariantCulture) + "\n"
                    + "Yaw=" + (cam.Yaw).ToString("############0.0#############", System.Globalization.CultureInfo.InvariantCulture) + "\n";
                byte[] sendBuffer = Encoding.ASCII.GetBytes(s);
                if (sendBuffer.Length > 0)
                    clientSocket.Send(sendBuffer);
            }*/

    }
}
