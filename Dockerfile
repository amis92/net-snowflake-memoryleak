FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
COPY SnowflakeMemoryLeak.csproj ./
ARG SnowflakePackageVersion=
RUN dotnet restore
COPY *.cs ./
RUN dotnet publish --no-restore -c Release -o out

# runtime
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "SnowflakeMemoryLeak.dll" ]