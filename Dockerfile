FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY VehicleDeclarations.csproj .
RUN dotnet restore VehicleDeclarations.csproj
COPY . .
RUN dotnet publish VehicleDeclarations.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/storage/uploads
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VehicleDeclarations.dll"]
