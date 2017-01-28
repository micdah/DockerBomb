# DockerBomb
Example code to demonstrate an issue with Docker for Windows under high concurrent load

## prerequisites
The code is written in .Net and requires *.Net Core*, the easiest way is just to intsall the command line tools from:
* [microsoft.com/net/core](https://www.microsoft.com/net/core)

Of course, if you have Visual Studio installed, this will also run it just fine.

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
```
ERROR: for redis  Cannot start service redis: driver failed programming external connectivity on endpoint dockerbomb_redis_1 (655ca14291eb9b761ee5ba18d512f5f4b3b59324c3bbe347d28a5980f0be9119): Error starting userland proxy: mkdir /port/tcp:0.0.0.0:6379:tcp:172.19.0.2:6379: input/output error
ERROR: No containers to start
```

## Demonstration video
[![YouTube video](https://img.youtube.com/vi/v5k1D60h0zE/0.jpg)](https://www.youtube.com/watch?v=v5k1D60h0zE)