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
        string resourceGroupName = config.Require("aksRgName");
        var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
        {
            // override autonaming https://www.pulumi.com/docs/intro/concepts/resources/#autonaming
            ResourceGroupName = resourceGroupName
        });

        // Create an AKS (https://www.pulumi.com/registry/packages/azure-native/api-docs/containerservice/managedcluster/)
        string aksName = config.Require("aksName");        
        string nodeResourceGroupName = config.Require("aksRgNodesName");
        var jjaks = new ManagedCluster(aksName, new ManagedClusterArgs
        {
            ResourceName = aksName,
            DnsPrefix = aksName,
            ResourceGroupName = resourceGroup.Name,
            NodeResourceGroup = nodeResourceGroupName,            
            AgentPoolProfiles = new[]
            {
                new ManagedClusterAgentPoolProfileArgs
                {                    
                    Name = "agentpool",
                    Mode = "System",
                    MinCount = 1,
                    MaxCount = 3,
                    Count = 1,
                    VmSize = "Standard_B2s",
                    AvailabilityZones = new[] { "1", "2", "3" },
                    EnableAutoScaling = true
                }
            },
            // WindowsProfile = new ManagedClusterWindowsProfileArgs
            // {
            //     AdminUsername = "cloudadmin",
            //     //TODO: reference keyvault value
            //     AdminPassword = "Azureuser123!"
            // },
            EnableRBAC = true,
            //TODO: add AAD admin group
            Identity = new ManagedClusterIdentityArgs
            {
                Type = Pulumi.AzureNative.ContainerService.ResourceIdentityType.SystemAssigned
            },
            AddonProfiles =
            {
                { "kubeDashboard", new Pulumi.AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                {
                    Enabled = false,
                } },
                { "azurepolicy", new Pulumi.AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                {
                    Enabled = true,
                } }
                //TODO: log analytics addon
                // { "omsagent", new Pulumi.AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                // {
                //     Config = 
                //     {
                //         { "log_analytics_workspace_id", "REFERENCE-ID" },
                //     },
                //     Enabled = true,
                // } }
            }
            //TODO: network profile

        });
    }

}
