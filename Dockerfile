# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY OrleansServer/*.csproj OrleansServer/
COPY OrleansGrains/*.csproj OrleansGrains/
COPY OrleansGrainInterfaces/*.csproj OrleansGrainInterfaces/
RUN dotnet restore OrleansServer/OrleansServer.csproj --use-current-runtime

# copy everything else and build app
COPY OrleansServer/ OrleansServer/
COPY OrleansGrains/ OrleansGrains/
COPY OrleansGrainInterfaces/ OrleansGrainInterfaces/
RUN dotnet publish OrleansServer/ --use-current-runtime --self-contained false --no-restore -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "OrleansServer.dll"]