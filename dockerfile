FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app

COPY ProgCloud/ProgCloud.csproj ProgCloud/
RUN dotnet restore ProgCloud/ProgCloud.csproj

COPY . ./
RUN dotnet publish "ProgCloud/ProgCloud.csproj" -c Release -o out
 
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ProgCloud.dll"]