using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using AplicationInformationExchange.Model;
using AplicationInformationExchange.Serialization;

namespace AplicationInformationExchange.Communication
{
    public class Sender: Communicator
    {
        public Sender(string address, int port, int bufferSize = 10240) : base(address, port, bufferSize)
        {
        }

        public Message Send(Message message)
        {
            CreateSocket(out IPEndPoint endPoint, out Socket socket);
            using (socket)
            {
                socket.Connect(endPoint);
                socket.Send(_Serializer.Serialize(message));
                return ReadOne(socket);
            }
        }
    }
}