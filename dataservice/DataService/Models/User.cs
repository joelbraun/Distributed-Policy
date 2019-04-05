using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataService.Models
{
    public class User
    {
        public User() 
        {

            Id = ObjectId.GenerateNewId().ToString();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get;set;}

        public string QualifiedName { get; set; }

        public List<string> Roles { get; internal set; } = new List<string>();

    }
}