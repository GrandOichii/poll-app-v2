# Poll app
Web application for creating/voting for polls.

## Projects
### PollApp.Api
Backend service, provides endpoints for registering accounts, logging in using JWT tokens, creating polls, voting for polls and fetching specific polls.

Requires Docker containers for a MongoDB database and Redis cache to run.

To run, execute the following commands:
```sh
cd PollApp.Api
dotnet run
```

### PollApp.Api.Tests
Testing suite for the __PollApp.Api__ project. Provides controller, service and endpoint tests.

To test, execute the following commands:
```sh
cd PollApp.Api.Tests
dotnet test
```