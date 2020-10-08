# TVMaze Scraper

This project scrapes the TV Maze API for show info and cast info and stores it in its own Sql Server.
The results are then accessed by a call to /api/shows with paging support.

```
/api/shows
/api/shows?page=1&pageSize=10
```

# This solution

The solution is written in dotnet core 3.1. This is the current LTS version and is recommended at this moment. The previous version 2.1 is nearing end of life.

The TV Maze API is rate limited, this means only a certain amount of calls are allowed in period of time. To this end, Polly has been implemented to deal with transient errors and rate limiting. Polly makes sure the HttpClient automatically tries again after a wait period.

The scraping of the TV Maze API can be stopped at any given time. The next time the API starts up again, it will continue where it left off.

# How to use

- Clone the repository
- In the folder where the project file (*.csproj) is located run
```
dotnet run
```

# Prerequisites

- dotnet SDK 3.1
- Sql Server

# To improve
- ~~Add tests~~
- Add Docker support to make running the solution easier
