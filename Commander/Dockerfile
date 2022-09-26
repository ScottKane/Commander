FROM steamcmd/steamcmd AS base
RUN dpkg --add-architecture i386 \
    && apt-get update \
    && apt-get install -y lib32gcc1 lib32stdc++6 libcurl4-gnutls-dev:i386
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Commander/Commander.csproj", "Commander/"]
RUN dotnet restore "Commander/Commander.csproj"
COPY . .
WORKDIR "/src/Commander"
RUN dotnet build "Commander.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Commander"
RUN dotnet publish "Commander.csproj" -c Release -o /app/publish -r linux-x64 -p:PublishSingleFile=true --self-contained

FROM base AS final
COPY --from=publish /app/publish .
ENV PATH="/app:${PATH}"
ENV STEAMCMD_LOCATION="/usr/bin"
ENV STEAMCMD_USERNAME="anonymous"

EXPOSE 8766/udp 8767/udp 16261/udp 16262-16272 27015

ENTRYPOINT ["commander"]