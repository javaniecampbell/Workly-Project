using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Workly.Core
{
    public class AzureDevOpsApiService
    {
        private readonly string organizationUrl;
        private readonly string personalAccessToken;

        public AzureDevOpsApiService(string organizationUrl, string personalAccessToken)
        {
            this.organizationUrl = organizationUrl;
            this.personalAccessToken = personalAccessToken;
        }
        private VssConnection GetVssConnection()
        {
            var basicCredential = new VssBasicCredential(string.Empty, personalAccessToken);
            var orgUri = new Uri(organizationUrl);
            VssConnection connection = new VssConnection(orgUri, basicCredential);
            return connection;
        }
    }
}
