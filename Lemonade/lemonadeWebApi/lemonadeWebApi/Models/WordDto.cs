using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace lemonadeWebApi.Models
{
    [DataContract]
    public class WordDto 
    {
        [BsonId]
        [DataMember]
        public string Word{ get; set; }
        [DataMember]
        public int Count{ get; set; }

    }
}
