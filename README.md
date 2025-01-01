# iceland

Generates database objects from SQL Server database

## Dev Setup Local MSSQL server

- Download install Docker.
- Pull latest image for MSSqlserver from docker registry `docker pull mcr.microsoft.com/mssql/server:latest`

- Start Container with default password: d`ocker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrongPassword@12345" -e "MSSQL_PID=Evaluation" -p 1433:1433 --name sqlserver-local --hostname sqlpreview -d mcr.microsoft.com/mssql/server:latest`
- Create database from script in sample-db directory
- config.json file by default points to local instance of sql server, update connection string to point to any sql server database.
- Run the iceland should see the updated code into `generated/` directory.

### Local install

iceland/.config/dotnet-tools.json

### Links

- https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create
