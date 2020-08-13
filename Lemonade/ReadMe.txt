
Overview
====================
For my understanding the task had two key considerations.
1) process multiple request with big data need some memory considerations
2) performance is important (parse gigabytes in 1-2 seconds). 

consideration #1 apporach:
	- Working with streams -  weather it is a url stream or file stream we can't load all the string to memory
	- aspnet garbage collector instructions - should provide help if needed
	- limit buffers - all buffers and aggragators are limited by configurable size

consideration #2 apporach:
	- need to allocate as little as possible and keep the memory continues - working with span which are continues in memory and passed by ref
	- work multithreaded and async - using TPL dataFlows and tasks. asyncing all the way up
	- stream is a bottleneck  - need to keep the stream working as much as possible
	- indexing DB  -  using the name as the index and splitting the tables to reduce in memory search

Design
=====================
Inheritance - All three requests (string, file, url) are basicly the same it's just the source that is different - using abstract class for all three situations

Pipeline Approach - stream -> word - >aggragate -> save (this shows in the file structure)



Important Files
=======================

appSettings.json -  you could find the StreamHandlerConfigurations. where you config the memory usage of the app

startup.cs - configure dependency injection and run the app

StreamHandlerBase.cs - 
			Receive stream and buffer it to aggragators.
			All stream handlers are using it abstraction.

WordsAggragator.cs - holds a dictionary and max_limit property. once AddWord is called. we update the dictionary.
			If we reached our acumulation limit. StartSaveAsync() is called which writes to Archiver.
			Archiving words to collection named with their first letter. (captain - > collection named c)
			for numbers and words starting with no letters we archive to collection named '~'

MongoArchiver.cs - provide the api to save the data to mongo (has some magic numbers that should move to settings)

HandlersRunner.cs - holds and run the Handlers

Files - holds some files for testing

usintest1.cs - place to run the services without web as mediator.


Assumptions
======================
1) Due to server limitation we are mostly bound to 2-4 GB size limitation in POST requests. The api receiving string  *Not* supports big objects (tens of GB).
We could use (multipart\from-data content-type request)\(api allowing multiple request)\(conduct webSocket using SignalR). this need some integration with the client
I started to work onthe multipart as it needs the minor integration but web connection issues will make the request to fail on big objects

2) Input is in english. and we are not case sensitive

3) All punctuation marks are word separators for example:
"kwahi.kwahi@gmail.com" should return kwahi = 2 , gmail = 1,com = 1 "data-sceince" should return data=1, science = 1

4) the ap will run on server with low resources


To Run
========================
1) make sure you have mongo server with "lemonade" db configured
2) run the "exe" from publish folder. (the app is self-contained and does not need .net core installed on the computer but I didn't test it on clean computer)
3) if port is busy you can change it in launchsettings.json
4) web-browser should be opened on startup and show how to conduct a requst. 