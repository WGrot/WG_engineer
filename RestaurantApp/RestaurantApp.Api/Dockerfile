FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RestaurantApp.Api/RestaurantApp.Api.csproj", "RestaurantApp.Api/"]
COPY ["RestaurantApp.Shared/RestaurantApp.Shared.csproj", "RestaurantApp.Shared/"]
COPY ["RestaurantApp.Infrastructure/RestaurantApp.Infrastructure.csproj", "RestaurantApp.Infrastructure/"]
COPY ["RestaurantApp.Application/RestaurantApp.Application.csproj", "RestaurantApp.Application/"]
COPY ["RestaurantApp.Domain/RestaurantApp.Domain.csproj", "RestaurantApp.Domain/"]
RUN dotnet restore "RestaurantApp.Api/RestaurantApp.Api.csproj"
COPY . .
WORKDIR "/src/RestaurantApp.Api"
RUN dotnet build "RestaurantApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RestaurantApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantApp.Api.dll"]
