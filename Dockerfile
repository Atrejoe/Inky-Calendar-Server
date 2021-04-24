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
# Don't have aclue about the repercussions
# https://github.com/dotnet/SqlClient/issues/220#issue-498595465
RUN apk update
RUN apk add icu-libs
RUN apk add ghostscript
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "InkyCal.Server.dll"]