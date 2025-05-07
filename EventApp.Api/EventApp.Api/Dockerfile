ARG DOTNET_VERSION=9.0
ARG ASPNET_VERSION=9.0

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY *.sln .

COPY ./EventApp.Api/EventApp.Api/ ./EventApp.Api/
COPY ./EventApp.Api/EventApp.Data/ ./EventApp.Data/
COPY ./EventApp.Api/EventApp.Models/ ./EventApp.Models/

RUN dotnet restore "./EventApp.Api/EventApp.Api.csproj"

WORKDIR "/src/EventApp.Api"
RUN dotnet publish "EventApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап создания конечного образа
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY ./certs/aspnetapp.pfx /https/aspnetapp.pfx

EXPOSE 8080
EXPOSE 8081 

# Точка входа
#ENTRYPOINT ["sh", "-c", "echo '--- Files in /https ---' && ls -l /https/ && echo '--- Environment variables ---' && printenv && echo '--- Sleeping to keep container alive ---' && sleep infinity"]
ENTRYPOINT ["dotnet", "EventApp.Api.dll"]
