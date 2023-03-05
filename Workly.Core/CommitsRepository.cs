using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;
using Workly.Core.Helpers;
using Workly.Core.Models;

namespace Workly.Core
{
    public class CommitsRepository
    {
        public async Task<bool> BulkInsertAsync(IEnumerable<Commit> commits)
        {
            bool isSuccesful = false;
            using (SqlConnection connection = new SqlConnection(GetConnetionString()))
            {
                await connection.OpenAsync();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity
                                                                          | SqlBulkCopyOptions.FireTriggers
                                                                          | SqlBulkCopyOptions.KeepNulls
                                                                          | SqlBulkCopyOptions.TableLock
                                                                          | SqlBulkCopyOptions.CheckConstraints, null)
                    )
                {
                    bulkCopy.DestinationTableName = "dbo.Commits";
                    bulkCopy.ColumnMappings.Add("CommitId", "CommitId");
                    bulkCopy.ColumnMappings.Add("Subject", "Subject");
                    bulkCopy.ColumnMappings.Add("CommitMessage", "CommitMessage");
                    bulkCopy.ColumnMappings.Add("AuthorName", "AuthorName");
                    bulkCopy.ColumnMappings.Add("AuthorEmail", "AuthorEmail");
                    bulkCopy.ColumnMappings.Add("AuthorDate", "AuthorDate");
                    bulkCopy.ColumnMappings.Add("Body", "Body");
                    bulkCopy.ColumnMappings.Add("CommitterName", "CommitterName");
                    bulkCopy.ColumnMappings.Add("CommitterEmail", "CommitterEmail");
                    bulkCopy.ColumnMappings.Add("CommitterDate", "CommitterDate");
                    bulkCopy.ColumnMappings.Add("FileChangeAdded", "FileChangeAdded");
                    bulkCopy.ColumnMappings.Add("FileChangeEdited", "FileChangeEdited");
                    bulkCopy.ColumnMappings.Add("FileChangeDeleted", "FileChangeDeleted");
                    bulkCopy.ColumnMappings.Add("RepositoryName", "RepositoryName");
                    bulkCopy.ColumnMappings.Add("RepositoryId", "RepositoryId");

                    bulkCopy.BatchSize = 10000;
                    bulkCopy.NotifyAfter = 10000;
                    bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler((s, a) => Console.WriteLine(a.RowsCopied));

                    bulkCopy.WriteToServer(commits.ToDataTable());

                }

            }

            return isSuccesful;
        }

        private string GetConnetionString()
        {
            return "Server=127.0.0.1,1433;Database=WorklyDev001;User Id=sa;Password=PMS@tr3d;Trusted_Connection=False;MultipleActiveResultSets=true;";
        }


        public IEnumerable<SqlDataRecord> CreateRecords(IEnumerable<Commit> commits)
        {
            /*
              CommitId VARCHAR(50) NOT NULL PRIMARY KEY,
		Subject VARCHAR(255) NOT NULL,
		[CommitMessage] VARCHAR(MAX) NOT NULL,
		AuthorName VARCHAR(255) NOT NULL,
		AuthorEmail VARCHAR(255) NOT NULL,
		AuthorDate DATETIME2 NOT NULL,
		Body VARCHAR(MAX) NULL,
		CommitterName VARCHAR(255) NULL,
		CommitterEmail VARCHAR(255) NULL,
		CommitterDate DATETIME2 NULL
            FileChangeAdded BIGINT NULL,
		 FileChangeEdited BIGINT NULL,
		 FileChangeDeleted BIGINT NULL
            RepositoryName VARCHAR(250) NULL,
		 RepositoryId VARCHAR(50) NULL

            */
            SqlMetaData[] scehma = new SqlMetaData[]
            {
                new SqlMetaData("CommitId",SqlDbType.VarChar,50),
                new SqlMetaData("Subject",SqlDbType.VarChar,255),
                new SqlMetaData("CommitMessage",SqlDbType.VarChar),
                new SqlMetaData("AuthorName",SqlDbType.VarChar,255),
                new SqlMetaData("AuthorEmail",SqlDbType.VarChar,255),
                new SqlMetaData("AuthorDate",SqlDbType.DateTime2),
                new SqlMetaData("Body",SqlDbType.VarChar),
                new SqlMetaData("CommitterName",SqlDbType.VarChar,255),
                new SqlMetaData("CommitterEmail",SqlDbType.VarChar,255),
                new SqlMetaData("CommitterDate",SqlDbType.DateTime2),
                new SqlMetaData("FileChangeAdded",SqlDbType.BigInt),
                new SqlMetaData("FileChangeEdited",SqlDbType.BigInt),
                new SqlMetaData("FileChangeDeleted",SqlDbType.BigInt),
                new SqlMetaData("RepositoryName",SqlDbType.VarChar,250),
                new SqlMetaData("RepositoryId",SqlDbType.VarChar,50),
            };

            SqlDataRecord dataRecord = new SqlDataRecord(scehma);
            foreach (Commit commit in commits)
            {
                dataRecord.SetString(0, commit.CommitId);
                dataRecord.SetString(1, commit.Subject);
                dataRecord.SetString(2, commit.CommitMessage);
                dataRecord.SetString(3, commit.AuthorName);
                dataRecord.SetString(4, commit.AuthorEmail);
                dataRecord.SetDateTime(5, commit.AuthorDate);
                dataRecord.SetString(6, commit.Body);
                dataRecord.SetString(7, commit.CommitterName);
                dataRecord.SetString(8, commit.CommitterEmail);
                dataRecord.SetDateTime(9, (DateTime)commit.CommitterDate);
                dataRecord.SetInt64(10, (long)commit.FileChangeAdded);
                dataRecord.SetInt64(11, (long)commit.FileChangeEdited);
                dataRecord.SetInt64(12, (long)commit.FileChangeDeleted);
                dataRecord.SetString(13, commit.RepositoryName);
                dataRecord.SetString(14, commit.RepositoryId);

                yield return dataRecord;
            }
        }
    }
}