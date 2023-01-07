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
        private int countUser;
        private bool userIsConnected = false;
        private ChatMessage message = new ChatMessage {
            //UserId = Guid.NewGuid().ToString(),
            MessageType = TypeMessage.Message
        };
        public ChatServer()
        {
            client = new UdpClient(3344);
        }
        public void Join(IPEndPoint endPoint, string userName)
        {
            if (members.Count() < countUser)
            {
                members.Add(endPoint);
                message.UserName = userName;
                message.Text = "Entered the chat.";
                message.DateTime = DateTime.Now.ToString();
                SendMessage(message);
            }
            else {                
                message.UserName = "Server";               
                message.Text = "Exceeding the number of users.";
                message.DateTime = DateTime.Now.ToString();
                SendMessage(message, endPoint);
            }
        }
        public void Leave(IPEndPoint endPoint, string userName)
        {            
            members.Remove(endPoint);
            message.UserName = userName;
            message.Text = "Logged out of the chat.";
            message.DateTime = DateTime.Now.ToString();
            SendMessage(message);

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
            countUser = int.Parse(Console.ReadLine());
            while (true)
            {
                try
                {
                    userIsConnected = false;
                    Console.WriteLine("Waiting for a request...");
                    byte[] request = client.Receive(ref clientEndPoint);
                    //string message = Encoding.UTF8.GetString(request);
                    //ChatMessage chatMessage = new ChatMessage();
                    //string message = request;

                    foreach (var member in members)
                    {
                        if (clientEndPoint == member)
                            userIsConnected = true;
                    }

                    var message = ChatMessage.Desserialize(request);
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