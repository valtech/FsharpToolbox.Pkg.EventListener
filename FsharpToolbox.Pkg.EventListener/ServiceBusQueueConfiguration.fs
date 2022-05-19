namespace FsharpToolboxPkg.EventListener

[<CLIMutable>]
type ServiceBusQueueConfiguration = {
    ConnectionString: string
    QueueName: string
}
