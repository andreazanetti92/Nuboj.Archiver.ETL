#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Nuboj.Archiver.ETL.Cleaner/Nuboj.Archiver.ETL.Cleaner.csproj", "Nuboj.Archiver.ETL.Cleaner/"]
RUN dotnet restore "Nuboj.Archiver.ETL.Cleaner/Nuboj.Archiver.ETL.Cleaner.csproj"
COPY . .
WORKDIR "/src/Nuboj.Archiver.ETL.Cleaner"
RUN dotnet build "Nuboj.Archiver.ETL.Cleaner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nuboj.Archiver.ETL.Cleaner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nuboj.Archiver.ETL.Cleaner.dll"]