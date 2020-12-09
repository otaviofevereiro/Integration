using Integration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Test.Events
{
    public class AddressEvent : Event
    {
        public string Name { get; set; }
    }
}
