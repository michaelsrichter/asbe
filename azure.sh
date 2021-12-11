LOCATION=eastus
NAMESUFFIX=asbe-$RANDOM
RGNAME=rg-$NAMESUFFIX
SERVICBUSNAME=sb-$NAMESUFFIX
az group create --name $RGNAME --location $LOCATION
az servicebus namespace create -n $SERVICBUSNAME -g $RGNAME --sku Premium --capacity 1 -l $LOCATION

az servicebus namespace authorization-rule create --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name RootManageSharedAccessKey --rights Listen Manage Send
az servicebus namespace authorization-rule keys list --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name RootManageSharedAccessKey --query primaryConnectionString
az servicebus queue create --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name queue1

az servicebus queue create --resource-group rg-asbe-31784 --namespace-name sb-asbe-31784 --name queue1



