$env:AZURE_STORAGE_ACCOUNT="jjpulumistate"
$env:AZURE_STORAGE_KEY=$(az storage account keys list -n jjpulumistate --query [0].value -o tsv)
$env:PULUMI_CONFIG_PASSPHRASE="Azure-1234512345"

pulumi up
