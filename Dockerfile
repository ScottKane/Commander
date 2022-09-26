FROM steamcmd/steamcmd AS base
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
#RUN mv Commander commander
ENV PATH="/app:${PATH}"
ENV STEAMCMD_LOCATION="/usr/bin"

ENTRYPOINT ["commander"]