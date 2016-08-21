# hostsedit
Edit the Window's hosts file from the command line.

## To use
1. Grab the code from this project.
1. Build the solution using Visual Studio.
1. Add the exe to PATH.
1. In a command window with administrator privledges:
  1. Use 'hostsedit -a mywebsite.local' to add a line like '127.0.0.1 mywebsite.local' to the hosts file.
  1. Use 'hostsedit -a 127.0.0.1 mywebsite.local' to add a line like '127.0.0.1 mywebsite.local' to the hosts file.
  1. Use 'hostsedit -d mywebsite.local' to delete the line containing 'mywebsite.local'.
  1. Use 'hostsedit -s' to show the list of entries currently in the hosts file.
