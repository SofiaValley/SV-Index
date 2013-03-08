using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SVIndex.Crawlers;
using SVIndex.ViewModels;

namespace SVIndex.Persistence
{
    public static class JobPostExtensions
    {
        public class DbSettings
        {
            public int Month { get; set; }
        }

        private const string PostsName = "posts";

        public static string GetDatabaseName()
        {
            return string.Format("posts-{0}", DateTime.Now.Month);
        }

        public static MongoDatabase OpenDatabase()
        {
            var connectionString = "mongodb://SVIndex:Asdf1234@ds053317.mongolab.com:53317/sv-index";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();

            var db = server.GetDatabase("sv-index");

            return db;
        }

        public static void DropDatabase(this MongoDatabase db)
        {
            db.DropCollection(PostsName);
        }

        public static void AddPost(this MongoDatabase db, JobPost post)
        {
            var posts = db.GetCollection<JobPost>(PostsName);
            posts.Save(post);
        }

        public static IEnumerable<JobPost> GetPosts(this MongoDatabase db)
        {
            return db.GetCollection<JobPost>(PostsName).AsQueryable();
        }

        public static void Preserve(this MongoDatabase db, SVIndexByMonth indexByMonth)
        {
            db.GetCollection<SVIndexByMonth>("SVIndexByMonth").Save(indexByMonth);
        }
    }
}