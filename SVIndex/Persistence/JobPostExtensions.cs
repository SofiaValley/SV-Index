using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using SV_PLI.Crawlers;
using Dapper;

namespace SV_PLI.Persistence
{
    public static class JobPostExtensions
    {
        public static SQLiteConnection OpenDatabase(string fileName)
        {
            SQLiteConnection connection;
            if (File.Exists(fileName))
            {
                connection = new SQLiteConnection(String.Format("Data Source={0}", fileName));
                connection.Open();    
            }
            else
            {
                connection = new SQLiteConnection(String.Format("Data Source={0}", fileName));
                connection.Open();
                connection.Execute(
                    "CREATE TABLE post(post_id TEXT PRIMARY KEY, external BOOL, failed BOOL, ref_no TEXT, date DATE, categories TEXT, job_type TEXT, level TEXT, employment_type TEXT, title TEXT, details TEXT, location TEXT, organization TEXT, zaplata TEXT)");                
            }

            return connection;
        }

        public static void Write(this JobPost post, DbConnection connection)
        {
            connection.Execute(
                @"INSERT OR REPLACE INTO post(post_id, external, failed, ref_no, date, title, details, location, organization, zaplata) 
                    VALUES (@Id, @IsExternal, @IsFailed, @RefNo, @Date, @Title, @Details, @Location, @Organisation, @Zaplata)",
                new
                    {
                        post.Id,
                        post.IsExternal,
                        post.IsFailed,
                        post.RefNo,
                        post.Date,
                        post.Title,
                        post.Details,
                        post.Location,
                        post.Organisation,
                        post.Zaplata
                    });
        }

        public static IEnumerable<JobPost> Read(string fileName)
        {
            using (var connection = new SQLiteConnection(String.Format("Data Source={0}", fileName)))
            {
                connection.Open();
                return connection.Query<JobPost>(@"SELECT post_id AS Id, 
external AS IsExternal, 
failed AS IsFailed, 
ref_no AS RefNo, 
date as Date, 
categories AS Categories, 
job_type AS JobType, 
level AS Level, 
employment_type AS EmploymentType, 
title AS Title, 
details AS Details, 
location AS Location, 
organization AS Organisation, 
zaplata AS Zaplata
FROM post");
            }
        }
    }
}