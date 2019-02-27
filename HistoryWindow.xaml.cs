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
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace P2Pchatt
{
   
    public partial class HistoryWindow : Window
    {
        public Database Db;
        TextBinding TextBind = new TextBinding { Name = "No user specified", Text = " Conversations with " };

        public HistoryWindow()
        {
            InitializeComponent();
            textBox.Focus();
            DataContext = TextBind;
        }
  
        //If something is written in the textBox perform a search in the "chats" table in the database
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Db = new Database();
            List<string> items = Db.RetrieveFromChatHistory("chats");
            //Delivers the result that starts with the text in the textbox
            var result = items.Where(x => x.StartsWith(textBox.Text));
            listBox.ItemsSource = result;
        }

        //Selects a username from the list and display all conversation with that user
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            
            string username = listBox.SelectedItem.ToString();
            TextBind = new TextBinding { Name = username, Text = " Conversations with " };
            DataContext = TextBind;

            List<string> messages = Db.RetrieveFromChatHistory(username);
            DataPacket packet;
            ConvoBox.Items.Clear();

            foreach (string name in messages)
            {
                packet = JsonConvert.DeserializeObject<DataPacket>(name);
                ConvoBox.Items.Add(packet.DT + " " + packet.Username + ": " + packet.Message);
            }
        }
    }
}
