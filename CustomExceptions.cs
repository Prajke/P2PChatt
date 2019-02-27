using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2Pchatt
{
    class CustomExceptions
    {
        //If the packagetype does not exist throw this exception
        public class InvalidPacketType : Exception
        {
            public InvalidPacketType()
                : base("This PacketType does not exist")
            {
            }
        }
    }
}
