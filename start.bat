::start database server, requires Docker
docker-compose up -d db

::starts web application, restarts upon changes
dotnet watch --project InkyCal.Server run