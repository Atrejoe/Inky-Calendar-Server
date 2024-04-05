#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

RUN apk -v update      && \
	apk -v upgrade

ARG USER=nonroot
WORKDIR /app

# add new user
RUN adduser -D $USER \
        && mkdir -p /etc/sudoers.d \
        && echo "$USER ALL=(ALL) NOPASSWD: ALL" > /etc/sudoers.d/$USER \
        && chmod 0440 /etc/sudoers.d/$USER


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InkyCal.Models/."       , "InkyCal.Models/."       ]
COPY ["InkyCal.Server/."       , "InkyCal.Server/."       ]
COPY ["InkyCal.Server.Config/.", "InkyCal.Server.Config/."]
COPY ["InkyCal.Utils/."        , "InkyCal.Utils/."        ]
COPY ["InkyCal.Data/."         , "InkyCal.Data/."         ]
#COPY ["*.sln", "."]
RUN dotnet restore "InkyCal.Server/InkyCal.Server.csproj"
COPY . .
WORKDIR "/src/InkyCal.Server"
RUN dotnet build "InkyCal.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InkyCal.Server.csproj" -c Release -o /app/publish --no-self-contained -r linux-musl-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Required for Pdf renderer
RUN apk -v add ghostscript

# Required for Time zone information
# https://github.com/dotnet/dotnet-docker/issues/1366#issuecomment-601888662
RUN apk -v add tzdata

# Addresses some globalization issue in the database
# Don't have a clue about the repercussions
# https://github.com/dotnet/SqlClient/issues/220#issue-498595465
RUN apk -v add icu-libs

# Remove cache from image, making it smaller
RUN apk cache clean

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false 

# CURL-based health check
# Used configured Asp Net Core url
HEALTHCHECK --interval=30s --timeout=30s --start-period=60s --retries=3 CMD wget --no-verbose --tries=1 --spider "$(echo $ASPNETCORE_URLS | cut -d ';' -f 1)health" || exit 1

USER $USER
ENTRYPOINT ["dotnet", "InkyCal.Server.dll"]