# Orleans Sandbox

This goal of this repository is to build a small sample [Orleans](https://github.com/dotnet/orleans) cluster running with [docker compose](https://docs.docker.com/compose/).


## Running the sample

1. Running `docker compose up` will start a three node Orleans cluster supported with Redis clustering (see [docker-compose.yml](/docker-compose.yml)).

2. Running `dotnet run` in the [OrleansClient](/OrleansClient/) directory will start the client that will allow you to send messages to the [`HelloGrain`](/OrleansGrains/HelloGrain.cs)s in the cluster.


## Issue

There is an issue with this sample.

For some reason I am unable to get the client application to successfully connect to all [three silo nodes in the cluster](/OrleansClient/Program.cs#L14-L17). It only successfully connects to the [first silo node](/OrleansClient/Program.cs#L11), via the default gateway port of `30000`.

In the [docker-compose.yml](/docker-compose.yml) file port mapping has been setup like:
```yml
# docker-compose.yml

  silo2:
    build: .
    ports:
      - "8081:8080"
      - "30001:30000" # <-- Should allow a client to connect to this silo via port 30001 without having to set this port on the silo configuration.

```

### Client error

When the client tries to establish a connection to the cluster using port `30001` (to talk to `silo2`) the client throws the following error:

```
info: Orleans.OutsideRuntimeClient[100314]
      Starting Orleans client with runtime version "7.1.2. Commit Hash: 01475f531c27c1c1fa7bc6f20827ffc8a50e5636+01475f531c27c1c1fa7bc6f20827ffc8a50e5636 (Release).", local address 172.25.0.1 and client id sys.client/2713f488650948229a946672ad6af1a5

info: Orleans.Messaging.GatewayManager[101309]
      Found 1 gateways: [gwy.tcp://127.0.0.1:30001/0]

info: Orleans.Runtime.Messaging.NetworkingTrace[0]
      Establishing connection to endpoint S127.0.0.1:30001:0

info: Orleans.Runtime.Messaging.NetworkingTrace[0]
      Connected to endpoint S127.0.0.1:30001:0

info: Orleans.Runtime.Messaging.NetworkingTrace[0]
      Established connection to S172.21.0.4:11111:43208830 with protocol version Version1

info: Orleans.Runtime.Messaging.NetworkingTrace[0]
      Connection [Local: 127.0.0.1:58684, Remote: 127.0.0.1:30001, ConnectionId: 0HMQLN0CCEMA8] established with S127.0.0.1:30001:0

warn: Orleans.Runtime.ClientClusterManifestProvider[0]
      Error trying to get cluster manifest from gateway S127.0.0.1:30001:0
      Orleans.Runtime.OrleansMessageRejectionException: Exception while sending message: Orleans.Runtime.Messaging.ConnectionFailedException: Unable to connect to endpoint S127.0.0.1:30001:0. See InnerException
 ---> Orleans.Networking.Shared.SocketConnectionException: Unable to connect to 127.0.0.1:30001. Error: ConnectionRefused
   at Orleans.Networking.Shared.SocketConnectionFactory.ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken) in /_/src/Orleans.Core/Networking/Shared/SocketConnectionFactory.cs:line 54
   at Orleans.Runtime.Messaging.ConnectionFactory.ConnectAsync(SiloAddress address, CancellationToken cancellationToken) in /_/src/Orleans.Core/Networking/ConnectionFactory.cs:line 61
   at Orleans.Runtime.Messaging.ConnectionManager.ConnectAsync(SiloAddress address, ConnectionEntry entry) in /_/src/Orleans.Core/Networking/ConnectionManager.cs:line 193
   --- End of inner exception stack trace ---
   at Orleans.Runtime.Messaging.ConnectionManager.ConnectAsync(SiloAddress address, ConnectionEntry entry) in /_/src/Orleans.Core/Networking/ConnectionManager.cs:line 221
   at Orleans.Runtime.Messaging.ConnectionManager.GetConnectionAsync(SiloAddress endpoint) in /_/src/Orleans.Core/Networking/ConnectionManager.cs:line 106
   at Orleans.Runtime.Messaging.MessageCenter.<SendMessage>g__SendAsync|30_0(MessageCenter messageCenter, ValueTask`1 connectionTask, Message msg) in /_/src/Orleans.Runtime/Messaging/MessageCenter.cs:line 224
         at Orleans.Serialization.Invocation.ResponseCompletionSource`1.GetResult(Int16 token) in /_/src/Orleans.Serialization/Invocation/ResponseCompletionSource.cs:line 230
         at System.Threading.Tasks.ValueTask`1.ValueTaskSourceAsTask.<>c.<.cctor>b__4_0(Object state)
      --- End of stack trace from previous location ---
         at Orleans.Runtime.ClientClusterManifestProvider.RunAsync() in /_/src/Orleans.Core/Manifest/ClientClusterManifestProvider.cs:line 92
```

### Silo error

When the client tries to establish a connection to the cluster using port `30001` (to talk to `silo2`) the silo (server) throws the following error:

```
orleanssandbox-silo2-1  | info: Orleans.Runtime.Messaging.Gateway[101301]
orleanssandbox-silo2-1  |       Recorded opened connection from endpoint 172.21.0.1:53502, client ID sys.client/2713f488650948229a946672ad6af1a5.

orleanssandbox-silo2-1  | info: Orleans.Runtime.Messaging.NetworkingTrace[0]
orleanssandbox-silo2-1  |       Establishing connection to endpoint S127.0.0.1:30001:0

orleanssandbox-silo2-1  | warn: Orleans.Runtime.Messaging.NetworkingTrace[0]
orleanssandbox-silo2-1  |       Connection attempt to endpoint S127.0.0.1:30001:0 failed
orleanssandbox-silo2-1  |       Orleans.Networking.Shared.SocketConnectionException: Unable to connect to 127.0.0.1:30001. Error: ConnectionRefused
orleanssandbox-silo2-1  |          at Orleans.Networking.Shared.SocketConnectionFactory.ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken) in /_/src/Orleans.Core/Networking/Shared/SocketConnectionFactory.cs:line 54
orleanssandbox-silo2-1  |          at Orleans.Runtime.Messaging.ConnectionFactory.ConnectAsync(SiloAddress address, CancellationToken cancellationToken) in /_/src/Orleans.Core/Networking/ConnectionFactory.cs:line 61
orleanssandbox-silo2-1  |          at Orleans.Runtime.Messaging.ConnectionManager.ConnectAsync(SiloAddress address, ConnectionEntry entry) in /_/src/Orleans.Core/Networking/ConnectionManager.cs:line 193
```

### Work around

The only way I have been successful at working around this issue is to set the gateway port number on each silo node to be different. This is done by adding an environment variable to the `docker-compose.yml` file that matches the port mapping.


```yml
  silo2:
    build: .
    ports:
      - "8081:8080"
      - "30001:30001" # <-- Map the same external to internal port
    environment:
      - Redis:ConnectionString=redis:6379
	  - Orleans:GatewayPort=30001 # <-- Set the same port number here
```

```csharp
	.Configure<EndpointOptions>(options =>
	{
		// Configure the gateway listening endpoint with the given port number
		
		var address = Dns.GetHostAddresses(Dns.GetHostName()).Single();
		var port = int.Parse(context.Configuration["Orleans:GatewayPort"] ?? "30000");

		options.GatewayListeningEndpoint = new IPEndPoint(address, port);
	})
```