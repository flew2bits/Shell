# Instructions for creating a new app using the shell

This is a sample shell for creating an app using the Decider Pattern with MartenDB and Docker.

Once you've downloaded the zip file from the Releases (in the right sidebar), in Windows, right click on the zip file, choose Properties, and 
check the Unblock checkbox. This will make it possible to run the PS1 scripts included in the file.

Unzip the file. Assuming you downloaded it to your Downloads folder, Windows will suggest unzipping to "C:\Users\\\[USERNAME]\Downloads\Shell-[VERSION]".
Delete the "Shell-[VERSION]" part -- there's already a folder named that in the zip file.

Rename the Shell-[VERSION] folder to your new project name. You can do this by clicking on the folder name in File Explorer and pressing F2.

Go into your newly renamed folder.

Right click on the "RenameSolution.ps1" file and select "Run with PowerShell". If after running this script nothing in the folder has been renamed, 
follow the instructions under "Allow PowerShell to run scripts", below.

At this point, you will have a functional program with a sample entity, Widget. To run it in docker, right click on "BuildDocker.ps1" and choose 
"Run with PowerShell". This will build the app, set up certificates as needed, and run the docker compose stack. You can visit your site at either 
http://localhost:8080 or https://localhost:8443. If you are going to use authorization, you'll need to set that up manually, and it will require the 
use of the https endpoint.

Once you have an understanding of how to create your own entity, you can delete the "Widget" and "Pages/Widget" folders, and remove `builder.Services.AddWidget()`  
and `app.MapWidgetApi()` from program.cs.

## Allow PowerShell to run scripts

Click on the Windows icon, and start typing "PowerShell". Click the ">" next to "Windows PowerShell" when it appears, and click on "Run as Administrator". Run 
the following command:

`Set-ExecutionPolicy RemoteSigned`

PowerShell will ask for confirmation. Type 'Y'. You should now be able to run the PowerShell scripts on your local machine.
