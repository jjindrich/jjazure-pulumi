using System.Threading.Tasks;
using Pulumi;

using AzureNative = Pulumi.AzureNative;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;

using Random = Pulumi.Random;

class JJAksStack : Stack
{
    // example https://github.com/pulumi/examples/tree/master/azure-cs-aks-helm
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

        // Use existing keyvault
        // ISSUE: cannot get secret value - GitHub Issue https://github.com/pulumi/pulumi-azure-native/issues/1422
        /*
        var nodeWindowsPassword = Output.Create(AzureNative.KeyVault.GetSecret.InvokeAsync(new AzureNative.KeyVault.GetSecretArgs
        {
            SecretName = "akswinpassword",            
            ResourceGroupName = config.Require("keyvault-group"),
            VaultName = config.Require("keyvault"),
        }));
        */
        var nodeWindowsPassword = new Random.RandomPassword("password", new Random.RandomPasswordArgs
        {
            Length = 16,
            Special = true,
            OverrideSpecial = "_%@",
        });
        Pulumi.Log.Info($"nodeWindowsPassword generated: {nodeWindowsPassword.Result}");

        // Use existing Virtual Network
        var aksSubnet = Output.Create(AzureNative.Network.GetSubnet.InvokeAsync(new AzureNative.Network.GetSubnetArgs
        {
            SubnetName = config.Require("NetworkSubnetName"),
            VirtualNetworkName = config.Require("NetworkName"),
            ResourceGroupName = config.Require("NetworkRgName"),
        }));

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
                    EnableAutoScaling = true,
                    VnetSubnetID = aksSubnet.Apply(s => s!.Id!), 
                }
            },
            WindowsProfile = new ManagedClusterWindowsProfileArgs
            {
                AdminUsername = "cloudadmin",
                AdminPassword = nodeWindowsPassword.Result,
            },
            EnableRBAC = true,
            //TODO: add AAD admin group
            Identity = new ManagedClusterIdentityArgs
            {
                Type = AzureNative.ContainerService.ResourceIdentityType.SystemAssigned
            },
            AddonProfiles =
            {
                { "kubeDashboard", new AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                {
                    Enabled = false,
                } },
                { "azurepolicy", new AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                {
                    Enabled = true,
                } }
                //TODO: log analytics addon
                // { "omsagent", new AzureNative.ContainerService.Inputs.ManagedClusterAddonProfileArgs
                // {
                //     Config = 
                //     {
                //         { "log_analytics_workspace_id", "REFERENCE-ID" },
                //     },
                //     Enabled = true,
                // } }
            },
            NetworkProfile = new AzureNative.ContainerService.Inputs.ContainerServiceNetworkProfileArgs
            {
                NetworkPlugin = "azure",
                LoadBalancerSku = "standard",
                OutboundType = "loadBalancer",
            },
        });
    }

}
