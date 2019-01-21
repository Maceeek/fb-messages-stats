using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbStatsApp
{
    class MessagesObject
    {
        public List<Participant> participants;
        public List<Message> messages;
    }

    class Message
    {
        public string sender_name;
        public string content;
        public long timestamp_ms;
    }

    class Participant
    {
       public string name;
    }

    

    
}
