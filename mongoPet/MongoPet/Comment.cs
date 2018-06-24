using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoPet
{
    public class Comment
    {
        public DateTime TimePosted { get; set; }

        public string Email { get; set; }

        public string Body { get; set; }
    }
}
