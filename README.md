# toggleclientserver
ToggleClientServer is a pair of apps that provide a simple toggle switch. One server exists. Multiple clients monitor the server and can change the toggle status.

Thanks!

TitaniumCoder477

Latest server download:
- https://github.com/TitaniumCoder477/ToggleClientServer/tree/master/ToggleServer/ToggleServer/bin/Release/ToggleServer.exe

Latest client download:
- https://github.com/TitaniumCoder477/ToggleClientServer/tree/master/ToggleClient/ToggleClient/bin/Release/ToggleClient.exe
- https://github.com/TitaniumCoder477/ToggleClientServer/tree/master/ToggleClient/ToggleClient/bin/Release/config.xml

Setup:
- Run the server app on a desktop/server
- Copy the client app and config file to a temp location
- Edit the config.xml file to reflect the resolvable name of the desktop/server
- Copy the client app (and config file) to other computers and run it
- The clients should easily find the server

Use:
- On a client computer, click the toggle to change the status to unavailable for 7 minutes
- Abort the status, i.e. reset it to available, by clicking on it again
