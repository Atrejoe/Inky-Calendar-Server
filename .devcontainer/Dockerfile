FROM mcr.microsoft.com/dotnet/sdk:10.0

ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

RUN apt-get update && apt-get install -y \
    ghostscript \
    tzdata \
    sudo \
    && rm -rf /var/lib/apt/lists/*

RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo "$USERNAME ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
