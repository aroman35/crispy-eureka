FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CrispyEureka.MarketDataConsumer/CrispyEureka.MarketDataConsumer.csproj", "CrispyEureka.MarketDataConsumer/"]
RUN dotnet restore "CrispyEureka.MarketDataConsumer/CrispyEureka.MarketDataConsumer.csproj"
COPY . .
WORKDIR "/src/CrispyEureka.MarketDataConsumer"
RUN dotnet build "CrispyEureka.MarketDataConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrispyEureka.MarketDataConsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrispyEureka.MarketDataConsumer.dll"]
