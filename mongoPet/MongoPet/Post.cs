using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;

namespace MongoPet
{
    public class Post
    {
        public Oid Id { get; private set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public int CharCount { get; set; }

        public IList<Comment> Comments { get; set; }
    }
}
