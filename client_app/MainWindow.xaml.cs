using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

 

namespace client_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChatMessage chatMessage = new ChatMessage
        {
            //UserId = Guid.NewGuid().ToString(),
            MessageType = TypeMessage.Login
        };
        const string serverIp = "127.0.0.1";
        const int serverPort = 3344;

        UdpClient client = new UdpClient();
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

        private bool isListening = false;
        private bool isConnecting = false;   

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Listen()
        {
            while (isListening)
            {
                try
                {
                    var response = await client.ReceiveAsync();
                    //string message = Encoding.UTF8.GetString(response.Buffer);
                    var message = ChatMessage.Desserialize(response.Buffer);

                    chatList.Items.Add($"{message.UserName}: {message.Text}  {message.DateTime} ");
                    if (message.Text == "Exceeding the number of users.")
                    {
                        btnSendMessage.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void SendMessage(ChatMessage message)
        {           
            try
            {
                //byte[] data = Encoding.UTF8.GetBytes(message);
                byte[] data = message.Serialize();
                client.SendAsync(data, data.Length, serverEndPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void JoinMenuClick(object sender, RoutedEventArgs e)
        {
            btnSendMessage.IsEnabled = true;
            chatMessage.UserName = nameTextBox.Text;
            chatMessage.MessageType = TypeMessage.Login;
            chatMessage.Text = "Add new user";
            chatMessage.DateTime = DateTime.Now.ToString();
            SendMessage(chatMessage);

            if (!isListening)
            {
                isListening = true;
                Listen();
            }
        }
        private void LeaveMenuClick(object sender, RoutedEventArgs e)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.MessageType = TypeMessage.Logout;
            chatMessage.Text = "<leave>";
            chatMessage.UserName = nameTextBox.Text;
            chatMessage.DateTime = DateTime.Now.ToString();
            SendMessage(chatMessage);
            isListening = false;
            isConnecting = false;
            btnSendMessage.IsEnabled = false;

        }
        private void SendBtnClick(object sender, RoutedEventArgs e)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.MessageType = TypeMessage.Message;
            chatMessage.Text = msgTextBox.Text;
            chatMessage.UserName = nameTextBox.Text;
            chatMessage.DateTime = DateTime.Now.ToString();

            if (string.IsNullOrWhiteSpace(chatMessage.Text)) return;

            SendMessage(chatMessage);
        }
        private void AboutMenuClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simple multi chat applicaiton using UDP protocol.", "About");
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isConnecting)
            {
                ChatMessage chatMessage = new ChatMessage();
                chatMessage.MessageType = TypeMessage.Logout;
                chatMessage.Text = "<leave>";
                chatMessage.UserName = nameTextBox.Text;
                chatMessage.DateTime = DateTime.Now.ToString();
                SendMessage(chatMessage);
                isListening = false;
            }
        }
    }
}
