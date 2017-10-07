# MatNet
An async websocket library for MATLAB using .NET Core framework.  
I built it as a middleware between a data visualization app which consist of a JavaScript frontend and MATLAB backend.


## MATLAB interface
To load the library, use `NET.addAssembly('path/to/MatNet.dll');`. This will import the MatNet namespace directly to MATLAB workspace.

You are then able to use:
* `MatNet.MatNet.Start();` to start the websocket server on port `1234`.
* `MatNet.MatNet.Stop();` to stop the websocket server.

The `MatNet` namespace exposes a `GenericMessage` type as well, used as a generic messages interface, which includes:
```
{
    Type: should be either 'PUSH' or 'STORE'
    ID: // some string
    Payload: // a struct-like object
}
```

You can initiate a msg with:
```
msg = MatNet.GenericMessage();
msg.Type = 'PUSH';
msg.ID = 'myID';
```
Alternatively, you can do `msg = MatNet.GenericMessage('PUSH', 'myID');`

To Add payload, do:
```
msg = MatNet.GenericMessage('PUSH', 'myID');
msg.Set('field1', [10, 20, 30, 40]);
msg.Set('field2', rand(3));
msg.Set('field3', {'a', 'b', 'c'});
x = msg.Get('field1'); % of NET.Object[] class
y = cellfun(@double, cell(msg.Get('field1'))); % a MATLAB vector
```

* To send a message, do `MatNet.MatNet.UpdatesQueue.Enqueue(msg);`
* To recieve commands, do `msg = MatNet.MatNet.UpdatesQueue.Dequeue();`

## TODO
* Add more Documentation for MATLAB side, spcifically about the queues interface and `GenericMessage`'s `Type` field.
* Add the abillity to `POST` to Rest-based server (ajax).