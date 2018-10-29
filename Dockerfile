#FROM microsoft/dotnet:2.1-sdk as build-env
FROM microsoft/dotnet:2.1-sdk as build-env
WORKDIR /app
FROM build-env as build-api
WORKDIR /WebApplication3
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app

FROM build-env as build-dependency
WORKDIR /WebApplication3.Data
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app

#FROM microsoft/dotnet:2.1-runtime
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=build-api /app .
COPY --from=build-dependency /app .
ENTRYPOINT ["dotnet", "WebApplication3.dll"]
