FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CrispyEureka.MarketDataConnector/CrispyEureka.MarketDataConnector.csproj", "CrispyEureka.MarketDataConnector/"]
RUN dotnet restore "CrispyEureka.MarketDataConnector/CrispyEureka.MarketDataConnector.csproj"
COPY . .
WORKDIR "/src/CrispyEureka.MarketDataConnector"
RUN dotnet build "CrispyEureka.MarketDataConnector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrispyEureka.MarketDataConnector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrispyEureka.MarketDataConnector.dll"]
