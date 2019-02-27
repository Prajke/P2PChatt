using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace P2Pchatt
{
    public class DataPacket
    {
        public int PackageType;
        public string Username;
        public string Message;
        public DateTime DT;
        public byte[] ImgByte;

        public DataPacket()
        {
        }
        
        [JsonConstructor]
        public DataPacket(int packagetype, string username, string message, byte[] imgbyte)
        {
            //Package Type defines which type of package it is
            // 0 = MessagePacket, 1 = DisconnectPackage, 2 = AcceptPackage, 3 = RequestPackage 4 = ImagePacket
            PackageType = packagetype;
            Username = username;
            Message = message;
            DT = DateTime.Now;
            ImgByte = imgbyte;

        }

    }
}
