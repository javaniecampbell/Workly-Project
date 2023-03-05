using CommitImporter.Properties;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Diagnostics;
using Workly.Core;
using Workly.Core.Models;

namespace CommitImporter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var azureDevOpService = new AzureDevOpsApiService(Resources.AZURE_DEVOPS_ORGANIZATION_URL, Resources.AZURE_DEVOPS_PAT);
            var commitsRepository = new CommitsRepository();
            var stopwatch = new Stopwatch();
            string[] repositories = new string[] {
                "dc1e47d0-4b54-4bec-a60c-6dd6ef15df91",
                "946fd4ff-c9b0-43ff-bc8e-9ee68b2dcc6f",
                "fee96637-9fc7-428c-8d25-bd50d3961325",
                "9993424a-e46b-47d7-a00c-f128402cbb71",
                "0970eff7-d8ea-4678-b02d-97c7486802aa"
            };

            var projectId = "40ef9a6b-d17b-4454-b6a6-3de33e04504f";

            Console.WriteLine("Retrieving all commits.....");
            stopwatch.Start();
            var commits = await azureDevOpService.GetAllCommits(projectId,repositories).ToListAsync();
            stopwatch.Stop();
            Console.WriteLine("Time taken for result: {0} seconds", stopwatch.ElapsedMilliseconds / 1000f);
            
            Console.WriteLine("Saving all commits.....");
            stopwatch.Restart();
            _ = await commitsRepository.BulkInsertAsync(commits);
            stopwatch.Stop();
            Console.WriteLine("Time taken for result: {0} seconds", stopwatch.ElapsedMilliseconds / 1000f);

        }
        #region Old Implementation for Reference

        private static async Task ArchiveAfterRefactor()
        {
            try
            {
                var stopwatch = new Stopwatch();
                Console.WriteLine("Retrieving all commits.....");
                stopwatch.Start();
                var commits = await GetAllCommits().ToListAsync();
                stopwatch.Stop();

                Console.WriteLine("Time taken for result: {0} seconds", stopwatch.ElapsedMilliseconds / 1000f);
                //await foreach (var commit in GetAllCommits().Take(3))
                //{


                //    Console.WriteLine("\nHash: {1}\nSubject: {0}\nBody: {2}\n\n", commit.Subject, commit.CommitId, commit.Body);
                //    //var count = await transformed.CountAsync();
                //    //if (count <= 0)
                //    //{
                //    //    Console.WriteLine("\nNo Commits to show for: {0}", repo);
                //    //}

                //    //Console.WriteLine("\n\nTotal Commits: {0} for Repo: {1}", count, repo);
                //}

                var repo = new CommitsRepository();
                Console.WriteLine("Saving all commits.....");

                stopwatch.Restart();
                _ = await repo.BulkInsertAsync(commits);
                stopwatch.Stop();
                Console.WriteLine("Time taken for result: {0} seconds", stopwatch.ElapsedMilliseconds / 1000f);
            }
            catch (Exception e)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error {0}", e.Message ?? e?.InnerException?.Message);
            }
            Console.ReadLine();
        }

        private static async IAsyncEnumerable<Commit> GetAllCommits()
        {
            string[] repos = new string[] {
                "dc1e47d0-4b54-4bec-a60c-6dd6ef15df91",
                "946fd4ff-c9b0-43ff-bc8e-9ee68b2dcc6f",
                "fee96637-9fc7-428c-8d25-bd50d3961325",
                "9993424a-e46b-47d7-a00c-f128402cbb71",
                "0970eff7-d8ea-4678-b02d-97c7486802aa"
            };
            var connection = GetVssConnection();
            using (var git = await connection.GetClientAsync<GitHttpClient>())
            {
                foreach (var repo in repos)
                {
                    var commits = await git.GetCommitsAsync("40ef9a6b-d17b-4454-b6a6-3de33e04504f", repo, new GitQueryCommitsCriteria
                    {
                        Top = Int32.MaxValue,
                    });

                    await foreach (Commit commit in TransformToModel(commits, repo))
                    {
                        yield return commit;
                    }

                }


            }
        }

        private static async IAsyncEnumerable<Commit> TransformToModel(List<GitCommitRef> commits, string repo)
        {
            var connection = GetVssConnection();
            // var result = new List<Commit>();
            using (var client = await connection.GetClientAsync<GitHttpClient>())
            {
                foreach (var commitRef in commits)
                {
                    var commit = await client.GetCommitAsync(commitRef.CommitId, repo);
                    var repository = await client.GetRepositoryAsync(repo);
                    //result.Add(new Commit
                    //{
                    //    CommitId = commitRef.CommitId,
                    //    AuthorName = commitRef.Author.Name,
                    //    AuthorEmail = commitRef.Author.Email,
                    //    AuthorDate = commitRef.Author.Date,
                    //    CommitterName = commitRef.Committer.Name,
                    //    Subject = commitRef.Comment.Split("\n")[0],
                    //    CommitterEmail = commitRef.Committer.Email,
                    //    CommitterDate = commitRef.Committer.Date,
                    //    Body = commit.Comment,
                    //    CommitMessage = commitRef.Comment,
                    //});
                    yield return new Commit
                    {
                        CommitId = commitRef.CommitId,
                        AuthorName = commitRef.Author.Name.Equals("=") ? commit.Push.PushedBy.DisplayName + " *MP" : commitRef.Author.Name,
                        AuthorEmail = commitRef.Author.Email.Equals("=") ? commit.Push.PushedBy.UniqueName + " *MP" : commitRef.Author.Email,
                        AuthorDate = commitRef.Author.Date,
                        CommitterName = commitRef.Committer.Name.Equals("=") ? commit.Push.PushedBy.DisplayName + " *MP" : commitRef.Committer.Name,
                        Subject = commitRef.Comment.Split("\n")[0],
                        CommitterEmail = commitRef.Committer.Email.Equals("=") ? commit.Push.PushedBy.UniqueName + " *MP" : commitRef.Committer.Email,
                        CommitterDate = commitRef.Committer.Date,
                        Body = commit.Comment,
                        CommitMessage = commitRef.Comment,
                        FileChangeAdded = commitRef.ChangeCounts[VersionControlChangeType.Add],
                        FileChangeDeleted = commitRef.ChangeCounts[VersionControlChangeType.Delete],
                        FileChangeEdited = commitRef.ChangeCounts[VersionControlChangeType.Edit],
                        RepositoryName = repository.Name,
                        RepositoryId = repository.Id.ToString(),
                    };
                }
            }


            //return result;
        }

        private static VssConnection GetVssConnection()
        {
            var basicCredential = new VssBasicCredential(string.Empty, Resources.AZURE_DEVOPS_PAT);
            var orgUri = new Uri(Resources.AZURE_DEVOPS_ORGANIZATION_URL);
            VssConnection connection = new VssConnection(orgUri, basicCredential);
            return connection;
        } 
        #endregion
    }
}