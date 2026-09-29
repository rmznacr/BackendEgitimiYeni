FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY BackendEgitimiYeni.csproj ./

RUN dotnet restore BackendEgitimiYeni.csproj

COPY . .

RUN dotnet publish BackendEgitimiYeni.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BackendEgitimiYeni.dll"]