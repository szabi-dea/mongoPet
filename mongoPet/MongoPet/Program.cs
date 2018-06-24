using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB;
using MongoDB.Linq;

namespace MongoPet
{
    class Program
    {
        // Test for User class

        private static UsersRepository _mongoDbRepo = new UsersRepository("mongodb://localhost:27017");

        static void Main(string[] args)
        {
            //Create a  mongo object
            //port 27017 
            var mongo = new Mongo();
            mongo.Connect();

            var db = mongo.GetDatabase("blog");

            var collection = db.GetCollection<Post>();

            //Delete all post
            collection.Delete(p => true);

            CreatePosts(collection);

            var totalNumberOfPosts = collection.Count();

            var numberOfPostsWith1Comment = collection.Count(p => p.Comments.Count == 2);

            var postsThatJaneCommentedOn = from p in collection.Linq()
                                           where p.Comments.Any(c => c.Email.StartsWith("Jane"))
                                           select p.Title;

            // filter by date
            var postsWithJanuary1st = from p in collection.Linq()
                                      where p.Comments.Any(c => c.TimePosted > new DateTime(2010, 1, 1))
                                      select new { Title = p.Title, Comments = p.Comments };

            //find posts with less than 40 characters
            var postsWithLessThan10Words = from p in collection.Linq()
                                           where p.CharCount < 40
                                           select p;


            //get the total character count for all posts...
            var sum = Convert.ToInt32(collection.MapReduce()
                .Map(new Code(@"
                    function() {
                        emit(1, this.CharCount);
                    }"))
                .Reduce(new Code(@"
                    function(key, values) {
                        var sum = 0;
                        values.forEach(function(prev) {
                            sum += prev;
                        });
                        return sum;
                    }"))
                .Documents.Single()["value"]);

            //Using Linq
            var linqSum = (int)collection.Linq().Sum(p => p.CharCount);

            var stats = from p in collection.Linq()
                        where p.Comments.Any(c => c.Email.StartsWith("bob"))
                        group p by p.CharCount < 40 into g
                        select new
                        {
                            LessThan40 = g.Key,
                            Sum = g.Sum(x => x.CharCount),
                            Count = g.Count(),
                            Average = g.Average(x => x.CharCount),
                            Min = g.Min(x => x.CharCount),
                            Max = g.Max(x => x.CharCount)
                        };


            // users
            CreateUser().ConfigureAwait(false);

            var users = GetAllUsers();

            var name = GetUserByFieldName();

            var blog = GetUserByFieldBlogName();

            InsertUser();

            DeleteUserById();
        }

        private static void CreatePosts(IMongoCollection<Post> collection)
        {
            var post = new Post()
            {
                Title = "My First Post",
                Body = "BlaBlaBLaBla",
                CharCount = 27,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2018,1,1), Email = "gipsz.jakab@gmail.com", Body = "Awesome" } },
                    { new Comment() { TimePosted = new DateTime(2018,1,2), Email = "kiss.janos@gmail.com", Body = "Second comment" } }
                }
            };

            //Save the post.
            collection.Save(post);

            //Get the first post that is not matching correctly
            post = collection.Linq().First(x => x.CharCount != x.Body.Length);

            post.CharCount = post.Body.Length;

            //this will perform an update this time because we have already inserted it.
            collection.Save(post);

            post = new Post()
            {
                Title = "My Second Post",
                Body = "BlaBlaBla",
                CharCount = 34,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2018,1,1), Email = "gipsz.jakabb@gmail.com", Body = "Egy fokkal jobb" } },
                }
            };

            //Save the post.  This will perform an upsert.
            collection.Save(post);

            post = new Post()
            {
                Title = "My Third Post",
                Body = "BllaBlabBla3",
                CharCount = 69,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2018,5,1), Email = "gipsz.jakabb@gmail.com", Body = "ASDASD" } },
                }
            };

            //This will perform an upsert.
            collection.Save(post);
        }

        private static async Task CreateUser()
        {
            var user = new User()
            {
                Name = "Béla",
                Age = 30,
                Blog = "vezess.hu",
                Location = "Hun"
            };
            await _mongoDbRepo.InsertUser(user);

            user = new User()
            {
                Name = "Gipsz",
                Age = 27,
                Blog = "index.hu",
                Location = "Hun"
            };
            await _mongoDbRepo.InsertUser(user);
        }

        public static async Task GetAllUsers()
        {
            var users = await _mongoDbRepo.GetAllUsers();
        }

        public static async Task GetUserByFieldName()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Béla");
        }

        public static async Task GetUserByFieldBlogName()
        {
            var users = await _mongoDbRepo.GetUsersByField("blog", "index.hu");
        }

        public static async Task InsertUser()
        {
            var user = new User()
            {
                Name = "Réka",
                Age = 19,
                Blog = "smink.hu",
                Location = "hun"
            };

            var users = await _mongoDbRepo.GetAllUsers();
            var countBeforeInsert = users.Count;

            await _mongoDbRepo.InsertUser(user);

            users = await _mongoDbRepo.GetAllUsers();
        }

        public static async Task DeleteUserById()
        {
            var user = new User()
            {
                Name = "Réka",
                Age = 19,
                Blog = "smink.hu",
                Location = "hun"
            };

            await _mongoDbRepo.InsertUser(user);

            var deleteUser = await _mongoDbRepo.GetUsersByField("name", "Réka");
            var result = await _mongoDbRepo.DeleteUserById(deleteUser.Single().Id);


        }

        public static async Task UpdateUser()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Béla");
            var user = users.FirstOrDefault();

            await _mongoDbRepo.UpdateUser(user.Id, "blog", "index2.hu");

            users = await _mongoDbRepo.GetUsersByField("name", "Béla");
            user = users.FirstOrDefault();
        }
    }
}
