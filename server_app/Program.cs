using Library;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace server_app
{
    public class ChatServer
    {
        private UdpClient client;
        private HashSet<IPEndPoint> members = new HashSet<IPEndPoint>();
        private int countConnectUsers;
        private List<string> listUserNames = new List<string>();
        private int countConnectUser;
        private int countUsers;
        private ChatMessage message = new ChatMessage
        {
            MessageType = TypeMessage.Message
        };
        public ChatServer()
        {
            client = new UdpClient(3344);
        }
        public void Join(IPEndPoint endPoint, string userName)
        {
            if (members.Count() < countConnectUsers)
            {
                bool repeatNameUser = false;
                foreach (var name in listUserNames)
                {
                    if (userName == name )
                    {
                        message.Text = "Enter a different name.";
                        message.UserName = "Server";                        
                        message.ListUsersName = listUserNames;
                        message.DateTime = DateTime.Now.ToString();
                        SendMessage(message, endPoint);
                        repeatNameUser = true;
                    }
                }
                if (!repeatNameUser)
                {
                    members.Add(endPoint);
                    listUserNames.Add(userName);
                    message.CountUsers =++countUsers;
                    message.Text = "Entered the chat.";
                    message.UserName = userName;
                    message.ListUsersName = listUserNames;
                    message.DateTime = DateTime.Now.ToString();                    
                    SendMessage(message);
                }
            }
            else {                
                message.UserName = "Server";
                message.CountUsers = 0;
                message.Text = "Exceeding the number of users.";                
                message.DateTime = DateTime.Now.ToString();
                SendMessage(message, endPoint);
            }
        }
        public void Leave(IPEndPoint endPoint, string userName)
        {            
            message.UserName = userName;            
            message.Text = "Logged out of the chat.";
            message.DateTime = DateTime.Now.ToString();
            message.CountUsers = --countUsers;
            SendMessage(message);
            members.Remove(endPoint);
            listUserNames.Remove(userName);
        }
        public void SendMessage(ChatMessage message)
        {
            //byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] data = message.Serialize();

            foreach (var member in members)
            {
                client.SendAsync(data, data.Length, member);
            }
        }
        public void SendMessage(ChatMessage message, IPEndPoint iPEndPoint)
        {
            //byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] data = message.Serialize();                       
            client.SendAsync(data, data.Length, iPEndPoint);           
        }
        public void Start()
        {
            IPEndPoint? clientEndPoint = null;
           
            Console.Write("Enter the maximum number of user =>");
            countConnectUsers = int.Parse(Console.ReadLine());
            while (true)
            {
                try
                {                    
                    Console.WriteLine("Waiting for a request...");
                    byte[] request = client.Receive(ref clientEndPoint);
                    //string message = Encoding.UTF8.GetString(request);
                    //ChatMessage chatMessage = new ChatMessage();
                    //string message = request;

                    //foreach (var member in members)
                    //{
                    //    if (clientEndPoint == member)
                    //        //userIsConnected = true;
                    //}

                    var message = ChatMessage.Desserialize(request);
                    message.ListUsersName = listUserNames;
                    message.CountUsers = countUsers;

                    Console.WriteLine($"Got a message: {message.Text} from {message.UserName}  {message.DateTime}");

                    switch (message.MessageType)
                    {
                        case TypeMessage.Login:
                            Join(clientEndPoint, message.UserName);
                            break;
                        case TypeMessage.Logout:
                            Leave(clientEndPoint, message.UserName);
                            break;
                        case TypeMessage.Message:
                            SendMessage(message);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {            
            ChatServer server = new ChatServer();
            server.Start();
        }
    }
}