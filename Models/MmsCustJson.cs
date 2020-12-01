using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MmsAdmin.Models
{
    public class MmsCustJson
    {
        public int ID { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public int CustomerId { get; set; }
        public string TimeSignedUp { get; set; }
        public string Language { get; set; }

    }
}
