# DockerBomb
Example code to demonstrate an issue with Docker for Windows under high concurrent load

## How to run
* Clone code into local folder
* Open command prompt and navigate to project root ```~\```
* Start redis container via ```docker-compose```
```
~\ # docker-compose up -d
```
* Navigate to ```~\src\Program``` and run
```
dotnet restore
dotnet build
dotnet run
```
* Enter number of threads to run (_try with 500_) and push enter
* Let it run, until all ```.``` have been replaced with ```x``` indicating all threads have died

When all the connections die simultaneously it seems to be because of an issue connecting to the docker container. 
All further attempts will fail (_which can be seen by trying to run the program again with any number of threads_),
only a restart of Docker fixes it.

It is also not possible to restart the container, as an error will occur when Docker tries to bind the exposed port 
on the host, although the port is not taken.
