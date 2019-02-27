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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Data.SQLite;
using Microsoft.VisualBasic;
using System.IO;
using Microsoft.Win32;

namespace P2Pchatt
{
    public partial class MainWindow : Window
    {
        IPAddress hostIp = IPAddress.Parse("127.0.0.1"); 
        public static SocketClass SocketClass= new SocketClass();
        public string MyUsername;
        public Database Db;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        //Enables the Send, Image and Disconnect buttons
        //Disables the Find-and Connect buttons
        //Activated when users are connected
        void Eventaction_EnableButtons(object sender, EventArgs e)
        {
            SendButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { SendButton.IsEnabled = true; }));
            ImageButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { ImageButton.IsEnabled = true; }));
            DisconnectButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { DisconnectButton.IsEnabled = true; }));
            FindButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { FindButton.IsEnabled = false; }));
            ConnectButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { ConnectButton.IsEnabled = false; }));
            
        }

        //Disables the Send, Image and Disconnect buttons
        //Enables the Find-and Connect buttons
        //Activated when users are disconnected
        void Eventaction_DisableButtons(object sender, EventArgs e)
        {
            SendButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { SendButton.IsEnabled = false; }));
            ImageButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { ImageButton.IsEnabled = false; }));
            DisconnectButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { DisconnectButton.IsEnabled = false; }));
            FindButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { FindButton.IsEnabled = true; }));
            ConnectButton.Dispatcher.BeginInvoke(new Action(delegate ()
            { ConnectButton.IsEnabled = true; }));
        
        }

        //Displays a image in the chat
        void Eventaction_DisplayImage(object sender, EventArgs e)
        {
            ImageSource image = sender as ImageSource;
            ConvoBox.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                Image img = new Image();
                img.Source = image;
                ConvoBox.Items.Add(img);
            }));  
        }

        //Displays a message in the chat
        void Eventaction_DisplayMessage(object sender, EventArgs e)
        {
            DataPacket packet = sender as DataPacket;
            Console.WriteLine("PACKET RECIEVED: " + packet.DT + " " + packet.Username + ": " + packet.Message);
            if(packet.PackageType == 2)
            {
                //Inits the database for the "client" user
                Db = new Database(MyUsername);
            }
            Db.AddToChatHistory(packet, MyUsername);
            ConvoBox.Dispatcher.BeginInvoke(new Action(delegate ()
            { ConvoBox.Items.Add(packet.DT + " " + packet.Username + ": " + packet.Message); }));
        }

        //Shows a MessageBox with a connection request.
        //Activates when the "Host" user recieves a connection
        void Eventaction_ConnectRequest(object sender, EventArgs e)
        {
            String ClientName = sender as String;
            
            MessageBoxResult MBResult = MessageBox.Show(ClientName + " want to chat with you. Accept?", "Connection request",MessageBoxButton.YesNo);
            if( MBResult == MessageBoxResult.Yes)
            {
                Console.WriteLine("Yes");
                //Inits the database for the "Host" user
                Db = new Database(MyUsername);
                DataPacket packet = new DataPacket(0, " ", "Connected, start chatting!", new byte[1]);
                Db.AddToChatHistory(packet, MyUsername);

                ConvoBox.Dispatcher.BeginInvoke(new Action(delegate ()
                { ConvoBox.Items.Add(packet.DT + " " +  packet.Message); }));
                Eventaction_EnableButtons(null, new EventArgs());

                //Sends a packet to the "Client" user that the connection has been accepted
                DataPacket DB= new DataPacket(2, "Accept Connection", "", new byte[1]);
                SocketClass.Send(DB);
            }
            else if(MBResult == MessageBoxResult.No)
            {
                Console.WriteLine("No");
                Eventaction_DisableButtons(null, new EventArgs());
                DataPacket packet = new DataPacket(1, "shutdown", "", new byte[1]);
                //Sends a packet to the "Client" user that the connection has been declined
                SocketClass.Send(packet);
                //The "Host" socket closes
                SocketClass.CloseSocket();
            }
        }

       ////////////////////////////////////////////////////// //BUTTONS////////////////////////////////////////////////////////////
        private void FindButton_Click(object Psender, RoutedEventArgs e)
        {
           try
            {
                //If the socket is null, create a new one.
                if(SocketClass.GetSocket() == null)
                {
                    SocketClass = new SocketClass();
                }

                ConvoBox.Items.Add("Waiting for a user to connect...");
                ConnectButton.IsEnabled = false;
                FindButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;

                MyUsername = UsernameHost.Text;
                IPEndPoint LocalEp = new IPEndPoint(hostIp, Convert.ToInt32(ListenPort.Text));
                SocketClass.Bind(LocalEp);

                //Starts listening after user to chat with              
                SocketClass.Listen(100);
                SocketClass.Accept();

                SocketClass.OnAccept += new EventHandler(Eventaction_ConnectRequest);
                SocketClass.OnDisconnect += new EventHandler(Eventaction_DisableButtons);
                SocketClass.OnReceiveMessage += new EventHandler(Eventaction_DisplayMessage);
                SocketClass.OnReceiveImage += new EventHandler(Eventaction_DisplayImage);

            }
            catch (SocketException ex)
            {
                Eventaction_DisableButtons(null, new EventArgs());
                MessageBox.Show("A socket error occurred: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A error occured: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }
       
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //If the socket is null, create a new one.
                if (SocketClass.GetSocket() == null)
                {
                    SocketClass = new SocketClass();
                }

                ConnectButton.IsEnabled = false;
                FindButton.IsEnabled = false;
                
                MyUsername = UsernameClient.Text;
                IPEndPoint RemoteEp = new IPEndPoint(IPAddress.Parse(IP.Text), Convert.ToInt32(Port.Text));
                SocketClass.OnReceiveMessage += new EventHandler(Eventaction_DisplayMessage);
                SocketClass.OnReceiveImage += new EventHandler(Eventaction_DisplayImage);

                //Connects to a specific user
                SocketClass.Connect(RemoteEp);

                //Sends a request to connect
                DataPacket DP = new DataPacket(3 ,MyUsername, "", new byte[1]);
                SocketClass.Send(DP);

                SocketClass.OnAccept += new EventHandler(Eventaction_EnableButtons);
                SocketClass.OnDisconnect += new EventHandler(Eventaction_DisableButtons);

            }
            catch (SocketException ex)
            {
                Eventaction_DisableButtons(null, new EventArgs());
                MessageBox.Show("A socket error occurred: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                Eventaction_DisableButtons(null, new EventArgs());
                MessageBox.Show("A error occured: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Creates a packet with the specific messages and sends it.
                DataPacket DP = new DataPacket(0, MyUsername, MessageLine.Text, new byte[1]);
                Db.AddToChatHistory(DP ,MyUsername);
                SocketClass.Send(DP);
                ConvoBox.Items.Add(DateTime.Now+ " Me: " + MessageLine.Text);
                MessageLine.Clear();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("A socket error occurred: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A error occured: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Send a DisconnectPackage to the other user and closes its socket
                ConvoBox.Items.Clear();
                DataPacket DP = new DataPacket(1, "Disconnect", "", new byte[1]);
                Db.AddToChatHistory(DP, MyUsername);
                SocketClass.Send(DP);
                Eventaction_DisableButtons(null, new EventArgs());
                SocketClass.CloseSocket();
            }
            catch(SocketException ex)
            {
                Eventaction_DisableButtons(null, new EventArgs());
                MessageBox.Show("A socket error occurred: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            catch(Exception ex)
            {
                Eventaction_DisableButtons(null, new EventArgs());
                MessageBox.Show("A error occured: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow w = new HistoryWindow();
            w.Show();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Opens a filedialog where a picture is chosen. (only works for smaller sizes, increase the receive buffer size to send larger files
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; )|*.jpg;";
               
                if (open.ShowDialog() == true)
                {   
                    //Specifices the image source
                    string ImgSrc = open.FileName;
                    byte[] imgBuff = File.ReadAllBytes(ImgSrc);
                   
                    DataPacket DP = new DataPacket(4, MyUsername, "Image", imgBuff);
                    Db.AddToChatHistory(DP, MyUsername);
                    SocketClass.Send(DP);

                    //Converts the imgbuffer to a picture that can be displayed in the chat
                    using (var ms = new System.IO.MemoryStream(imgBuff))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = ms;
                        image.DecodePixelHeight = 100;
                        image.DecodePixelWidth = 100;
                        image.EndInit();
                        image.Freeze();
                        
                        Eventaction_DisplayImage(image, new EventArgs());
                    }
                    ConvoBox.Items.Add(DateTime.Now + " Me: " + "Image");
                    MessageLine.Clear();
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("A socket error occurred: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A error occured: " + ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
