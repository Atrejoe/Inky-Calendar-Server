#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
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
RUN dotnet publish "InkyCal.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InkyCal.Server.dll"]
