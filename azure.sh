az extension add --source https://workerappscliextension.blob.core.windows.net/azure-cli-extension/containerapp-0.2.0-py2.py3-none-any.whl
az provider register --namespace Microsoft.Web

LOCATION=eastus
NAMESUFFIX=asbe-$RANDOM
RGNAME=rg-$NAMESUFFIX
SERVICBUSNAME=sb-$NAMESUFFIX
APPINSIGHTSNAME=ai-$NAMESUFFIX
LOGALANALYTICSWORKSPACE=law-$NAMESUFFIX
CONTAINERAPPSENVIRONMENT=ca-$NAMESUFFIX
az group create --name $RGNAME --location $LOCATION

az servicebus namespace create -n $SERVICBUSNAME -g $RGNAME --sku Premium --capacity 1 -l $LOCATION

az servicebus namespace authorization-rule create --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name RootManageSharedAccessKey --rights Listen Manage Send
SBCONNECTIONSTRING="$(az servicebus namespace authorization-rule keys list --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name RootManageSharedAccessKey --query primaryConnectionString) cd --out tsv"
az servicebus queue create --resource-group $RGNAME --namespace-name $SERVICBUSNAME --name queue1

az monitor log-analytics workspace create --resource-group $RGNAME -n $LOGALANALYTICSWORKSPACE -l $LOCATION
LOG_ANALYTICS_WORKSPACE_CLIENT_ID=`az monitor log-analytics workspace show --query customerId -g $RGNAME -n $LOGALANALYTICSWORKSPACE --out tsv`
LOG_ANALYTICS_WORKSPACE_CLIENT_SECRET=`az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $RGNAME -n $LOGALANALYTICSWORKSPACE --out tsv`

az monitor app-insights component create -g $RGNAME  --app $APPINSIGHTSNAME --location $LOCATION --retention-time 30 --application-type web --kind web
AIKEY="$(az monitor app-insights component show -a $APPINSIGHTSNAME -g $RGNAME --query instrumentationKey) --out tsv"


