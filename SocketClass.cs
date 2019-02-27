using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace P2Pchatt
{
    public class SocketClass
    {

        private Socket TempSocket;
        private byte[] buffer = new byte[256];

        public event EventHandler OnReceiveMessage;
        public event EventHandler OnReceiveImage;
        public event EventHandler OnAccept;
        public event EventHandler OnDisconnect;

        public SocketClass()
        {
            try
            {
                TempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        public Socket GetSocket()
        {
            return TempSocket;
        }
        public void CloseSocket()
        {
            Console.WriteLine("Socket is NULL");
            TempSocket.Shutdown(SocketShutdown.Both);
            TempSocket.Close();
            TempSocket = null;
        }

        //Converts the imgbuffer to a picture that can be displayed in the chat and saves it as recievedimg.jpg
        public void ImageLoader(byte[] img, DataPacket packet)
        {
            using (var ms = new System.IO.MemoryStream(img))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.DecodePixelHeight = 100;
                image.DecodePixelWidth = 100;
                image.EndInit();
                image.Freeze();

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                string photolocation = "recievedimg.jpg";  //file name 
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image));
                using (var filestream = new FileStream(photolocation, FileMode.Create))
                    encoder.Save(filestream);
                OnReceiveMessage(packet, new EventArgs());
                OnReceiveImage(image, new EventArgs());
            }
        }

        //Sends a DataPacket to the other connected user
        public void Send(DataPacket DP)
        {
            try
            {
                //Converts the package into a json string
                string packet = JsonConvert.SerializeObject(DP);
                //Convert the json string into bytes
                byte[] byteData = Encoding.UTF8.GetBytes(packet); //encoding UTF8

                //Sends the data
                TempSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback),
                    TempSocket);
            }
            catch(SocketException ex)
            {
                OnDisconnect(null, new EventArgs());
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        //Sends data when the other user is ready to recieve
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {           
                Socket client = (Socket)ar.AsyncState; 
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes ", bytesSent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        //Receives data from the other user
        private void Receive()
        {
            try
            {
                while (true)
                {
                    buffer = new byte[20000];
                    int size;
                    //Tries to recieve if not possible, check if the could because of a null socket.
                    try
                    {
                        size = TempSocket.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        if (TempSocket == null)
                        {
                            Console.WriteLine("Tempsocket is NULL, reset the socket to recieve data");
                            OnDisconnect(null, new EventArgs());
                            break;
                        }
                       
                        Console.WriteLine("Got Socket Exception, TempSocket is " + TempSocket);
                        return;
                    }
                    UTF8Encoding Encoding = new UTF8Encoding();
                    string message = Encoding.GetString(buffer, 0, size);
                    Console.WriteLine(message);
                    DataPacket packet = JsonConvert.DeserializeObject<DataPacket>(message);
                    
                    //Checks which packagetype it is and acts accordingly
                    //The different packagetypes are listed in the DataPacket class
                    if (packet.PackageType == 0)
                    {
                        OnReceiveMessage(packet, new EventArgs());
                    }
                    else if (packet.PackageType == 1)
                    {
                        Console.WriteLine("CLIENT disconnected");
                        DataPacket DP = new DataPacket(2, " ", "Chat disconnected.", new byte[1]);
                        OnReceiveMessage(DP, new EventArgs());
                        OnDisconnect(null, new EventArgs());
                        CloseSocket();
                    }
                    else if (packet.PackageType == 2)
                    {
                        Console.WriteLine("HOST accepted connection");
                        DataPacket DP = new DataPacket(2, " ", "Connected, start chatting!", new byte[1]);
                       
                        OnReceiveMessage(DP, new EventArgs());
                        OnAccept(packet.Username, new EventArgs());
                    }
                    else if (packet.PackageType == 3)
                    {
                        OnAccept(packet.Username, new EventArgs());
                    }
                    else if (packet.PackageType == 4)
                    {
                        Console.WriteLine("IMAGE RECEIVED");
                       
                        byte[] img = packet.ImgByte;
                        ImageLoader(img, packet);
                        
                    }
                    else if (packet.PackageType > 5 || packet.PackageType < 0)
                    {
                        throw new CustomExceptions.InvalidPacketType();
                    }
                }
            }
            catch (CustomExceptions.InvalidPacketType ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
           

        }

        /////////////////////////////////////////////////////CLIENT SOCKET/////////////////////////////////////////////////////////////////////

        //Connects to a specific user
        public void Connect(IPEndPoint TargetEp)
        {
            try
            {
                TempSocket.BeginConnect(TargetEp, new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        //Connect to user when its available
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
               if (TempSocket.Connected)
               {
                   Console.WriteLine("Connected");
                   Receive();
               }
               else
               {
                   Console.WriteLine("Not connected");
               }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        /////////////////////////////////////////////////////HOST SOCKET/////////////////////////////////////////////////////////////////////
      
        public void Bind(IPEndPoint TargetEp)
        {
            try
            {
                TempSocket.Bind(TargetEp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Listen(int MaxClients)
        {
            try
            {
                TempSocket.Listen(MaxClients);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Accept()
        {
            try
            {
                TempSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                TempSocket = TempSocket.EndAccept(ar);

                if (TempSocket.Connected)
                {
                    Console.WriteLine("User found! Connected");
                    Receive();  
                }
                else
                {
                    Console.WriteLine("Could not connect..");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
