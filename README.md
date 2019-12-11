# Git Search v2

## Description
Provides a full-text search of the commits contained in the LMS repo.

### Requirements
* [.Net Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/get-started?view=aspnetcore-3.1&tabs=visual-studio)
* [Visual Studio 16.4 or later](https://visualstudio.microsoft.com/vs/preview/)

### Optional
* SqlServer with Full-Text Search installed

### Operation
GitSearch2.Indexer is a console application that will perform the actual work of moving through the repository and indexing the commits.  

Simple example for a local Sqlite database:
```
GitSearch2.Indexer.exe --input c:\d2l\instances\dev2\checkout\.git --pause true --live true --database Sqlite --connection "Data Source=webapp.sqlite3"
```

The web server uses `appsettings.json` to control which database is used.

### Notes

The indexer can be run multiple times.  When the indexer runs it will attempt to determine if an indexing is already underway. If one is, it will schedule a follow-on indexing and then exit.
The currently running indexer, when it completes its current run, will check to see if any updates are scheduled and will re-execute automatically.   This loop will continue until there are no scheduled runs pending.
