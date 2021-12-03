::Publish Docker image, requires write access atreyu/inkycal.server
docker-compose build
docker tag inky-calendar-server_web:latest atreyu/inkycal.server:latest
docker push atreyu/inkycal.server:latest