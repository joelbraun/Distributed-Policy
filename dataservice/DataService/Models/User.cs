using System;
using System.Collections.Generic;
using System.Linq;

namespace DataService.Models
{
    public class User
    {
        public string Id { get; set; }

        public List<string> Roles { get; internal set; } = new List<string>();

    }
}