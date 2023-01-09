namespace Library
{    

    public class ChatMessage
    {
        public TypeMessage MessageType;
        //public string UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public string DateTime { get; set; }
        public int CountUsers { get; set; } = 0;
        public List<string> ListUsersName { get; set; } = new List<string>();              
        public byte[] Serialize()
        {
            using (var m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write((int)MessageType);
                    //writer.Write(UserId);
                    writer.Write(UserName);
                    writer.Write(Text);
                    writer.Write(DateTime);
                    writer.Write(CountUsers);

                    foreach (var name in ListUsersName)
                    {
                        writer.Write(name);
                    }
                }
                return m.ToArray();
            }
        }
        public static ChatMessage Desserialize(byte[] data)
        {
            ChatMessage message = new ChatMessage();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    message.MessageType = (TypeMessage)reader.ReadInt32();
                    //message.UserId = reader.ReadString();
                    message.UserName = reader.ReadString();
                    message.Text = reader.ReadString();
                    message.DateTime = reader.ReadString();
                    message.CountUsers = reader.ReadInt32();

                    for (int i = 0; i < message.CountUsers; i++)
                    {
                        message.ListUsersName.Add(reader.ReadString());
                    }


                }
            }
            return message;
        }
    }
}