using CommitImporter.Properties;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Workly.Core.Models;

namespace CommitImporter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await GetAllCommits();

            Console.ReadLine();
        }

        private static async Task GetAllCommits()
        {
            string[] repos = new string[] { "dc1e47d0-4b54-4bec-a60c-6dd6ef15df91", "946fd4ff-c9b0-43ff-bc8e-9ee68b2dcc6f", "fee96637-9fc7-428c-8d25-bd50d3961325", "9993424a-e46b-47d7-a00c-f128402cbb71", "0970eff7-d8ea-4678-b02d-97c7486802aa" };
            var connection = GetVssConnection();
            using (var git = connection.GetClient<GitHttpClient>())
            {
                foreach (var repo in repos)
                {
                    var commits = await git.GetCommitsAsync("40ef9a6b-d17b-4454-b6a6-3de33e04504f", repo, new GitQueryCommitsCriteria
                    {
                        Top = Int32.MaxValue,
                    });

                    var transformed = await TransformToModel(commits, repo);

                    var sample = transformed.Take(3);

                    sample.ForEach(x => Console.WriteLine("\nHash: {1}\nSubject: {0}\nBody: {2}\n\n",x.Subject,x.CommitId,x.Body));

                    if(transformed.Count() <= 0)
                    {
                        Console.WriteLine("\nNo Commits to show for: {0}", repo);
                    }
                   
                    Console.WriteLine("\n\nTotal Commits: {0} for Repo: {1}", transformed.Count(), repo);
                }

                
            }
        }

        private static async Task<List<Commit>> TransformToModel(List<GitCommitRef> commits, string repo)
        {
            var connection = GetVssConnection();
            var result = new List<Commit>();
            using (var client = connection.GetClient<GitHttpClient>())
            {
                foreach (var commitRef in commits)
                {
                    var commit = await client.GetCommitAsync(commitRef.CommitId, repo);
                    result.Add(new Commit
                    {
                        CommitId = commitRef.CommitId,
                        AuthorName = commitRef.Author.Name,
                        AuthorEmail = commitRef.Author.Email,
                        AuthorDate = commitRef.Author.Date,
                        CommitterName = commitRef.Committer.Name,
                        Subject = commitRef.Comment.Split("\n")[0],
                        CommitterEmail = commitRef.Committer.Email,
                        CommitterDate = commitRef.Committer.Date,
                        Body = commit.Comment,
                        CommitMessage = commitRef.Comment,
                    });
                }
            }
        

            return result;
        }

        private static VssConnection GetVssConnection()
        {
            var basicCredential = new VssBasicCredential(string.Empty, Resources.AZURE_DEVOPS_PAT);
            var orgUri = new Uri(Resources.AZURE_DEVOPS_ORGANIZATION_URL);
            VssConnection connection = new VssConnection(orgUri, basicCredential);
            return connection;
        }
    }
}