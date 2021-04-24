#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["InkyCal.Models/.", "InkyCal.Models/."]
COPY ["InkyCal.Server/.", "InkyCal.Server/."]
COPY ["InkyCal.Server.Config/.", "InkyCal.Server.Config/."]
COPY ["InkyCal.Utils/.", "InkyCal.Utils/."]
COPY ["InkyCal.Data/.", "InkyCal.Data/."]
COPY ["*.sln", "."]
RUN dotnet restore "InkyCal.Server/InkyCal.Server.csproj"
COPY . .
WORKDIR "/src/InkyCal.Server"
RUN dotnet build "InkyCal.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InkyCal.Server.csproj" -c Release -o /app/publish -r linux-musl-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Addresses some globalization issue in the database
# Don't have a clue about the repercussions
# https://github.com/dotnet/SqlClient/issues/220#issue-498595465

# Required for .Net on alpine
RUN apk add icu-libs

RUN apk add ghostscript
#RUN apk add musl-dev
#RUN apk add glibc 

# Required for .Net on alpine
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false 
ENV LD_DEBUG=all

ENTRYPOINT ["dotnet", "InkyCal.Server.dll"]

#RUN apt-get install --yes musl-dev
#RUN ln -s /usr/lib/x86_64-linux-musl/libc.so /lib/libc.musl-x86_64.so.1