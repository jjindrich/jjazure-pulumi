# JJ Azure automation with Pulumi

Pulumi intro
- Pulumi CLI https://github.com/pulumi/pulumi
- Project and Stack https://www.pulumi.com/docs/intro/concepts/#overview
- Azure Native Provider (it’s built and automatically from the Azure Resource Manager API) https://github.com/pulumi/pulumi-azure-native
- State https://www.pulumi.com/docs/intro/concepts/state/
- Language - I will use C# - create project https://www.pulumi.com/docs/get-started/azure/create-project/
- Convert Terraform https://www.pulumi.com/tf2pulumi/

## Prepare Pulumi

Install Pulumi and setup State store

```powershell
choco install pulumi
```

Pulumi using default State store api.pulumi.com https://www.pulumi.com/docs/intro/concepts/state/#pulumi-service-architecture

I want to use my own Azure Storage state store https://www.pulumi.com/docs/intro/concepts/state/#logging-into-the-azure-blob-storage-backend

Create new Azure Storage account name jjpulumistate with container name jjpulumi

```powershell
$env:AZURE_STORAGE_ACCOUNT="jjpulumistate"
$env:AZURE_STORAGE_KEY=$(az storage account keys list -n jjpulumistate --query [0].value -o tsv)
pulumi login azblob://jjpulumi
```

## Create new project and deploy

Create new project src-test - using Azure and C# template

Next setup password to access data in state.

```powershell
mkdir src-test && cd src-test
pulumi new azure-csharp

$env:PULUMI_CONFIG_PASSPHRASE="<HESLO>"
pulumi up
```