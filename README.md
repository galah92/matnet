# MatNet
An async websocket library for MATLAB using .NET Core framework.  
I built it as a middleware between a data visualization app which consist of a JavaScript frontend and MATLAB backend.

## MATLAB API

### Install

Run the following commands from the command line **as administrator**:
```
netsh http add iplisten 0.0.0.0
netsh http add urlacl url="http://+:1234/" user=Everyone
netsh advfirewall firewall add rule name="MatNet" dir=in action=allow protocol=TCP localport=1234
```

### Configuring

Assuming you've got the following:
* `...path/to/MatNet.dll`
* `...path/to/Newtonsoft.Json.dll`

Then in MATLAB, do `NET.addAssembly('...path/to/MatNet.dll');`  
It will load the `MatNet` variable to the MATLAB workspace.

### Methods and Properties

* `version = MatNet.Server.Version` : Get the current MatNet version.
* `MatNet.Server.Start()` : Start the server. Restart if called while the server is listening.
* `MatNet.Server.Stop()` : Stop the server.
* `MatNet.Server.ClientsCount` : Get the current number of connected clients.
* `MatNet.Server.Send(Message msg)` : Send a message to all connected clients.
* `MatNet.Server.Store(Message msg)` : Store a message to be fetched by clients.
* `msg = MatNet.Server.Recieve()` : Retrieve a message if there is one, otherwise `msg ` is empty.

### The `Message` Type

A message has `ID` and `Payload` fields.  
To construct a new message, do `msg = MatNet.Message()`

#### The ID
* `msg = MatNet.Message(id)` : Construct a new message with an ID.
* `msg.ID = id` : Assign a new ID to a message.
* `id = msg.ID` : Retrieve the current message ID.

#### The Payload
The `Payload` is a dictionary-like object consist of `key:value` pairs, where `key`s are of type `NET.String` (or MATLAB's `char`s), and `value`s are of `double`, `char`, `cell` MATLAB classes.
* `value = msg.Get('myKey')` : Retrieve the value of a certain key.
* `msg.Set('myKey') = value` : Assign a value to a certain key.
* `keys = msg.GetKeys()` : Retrieve all the keys of a certain message.

## TODO
* Add the abillity to `POST` to Rest-based server (ajax).
