# die-hard
Simple example of using Queues with Azure Service Bus

## Getting Started
Create an azure service bus namespace (free tier will suffice) then add a privateSettings.config file to the root of the src folder which holds the connection string to your service bus instance.
Example:
```
<?xml version="1.0" encoding="utf-8"?>
<appSettings>
    <!-- Service Bus specific app setings for messaging connections -->
    <add key="Microsoft.ServiceBus.ConnectionString"
        value="Endpoint=sb://[your namespace].servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your secret]"/>
</appSettings>
```


