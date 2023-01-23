:: Publish Docker image, requires write access atreyu/inkycal.server
docker-compose build --pull

:: Tag it as the latest version
::docker tag inky-calendar-server_web:latest atreyu/inkycal.server:latest
docker tag docker.io/library/inky-calendar-server-web atreyu/inkycal.server:latest

:: Scan for vulnerabilities
docker scan atreyu/inkycal.server:latest --file=Dockerfile

:: Publish
docker push atreyu/inkycal.server:latest