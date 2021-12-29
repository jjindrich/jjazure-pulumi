using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;

class JJAksStack : Stack
{
    public JJAksStack()
    {
        // access stack configuration
        var config = new Pulumi.Config();

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("jjmicroservices-rg");

        // Create an AKS
        string aksName = config.Require("aksName");
        var jjaks = new ManagedCluster(aksName, new ManagedClusterArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AgentPoolProfiles = new[]
            {
                new ManagedClusterAgentPoolProfileArgs
                {
                    Name = "agentpool",
                    Count = 3,
                    VmSize = "Standard_B2s"
                }
            }
        });
    }

}
