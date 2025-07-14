#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM microsoft.repo.stinascloud.ir/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/HodHod.Migrator/HodHod.Migrator.csproj", "src/HodHod.Migrator/"]
COPY ["src/HodHod.EntityFrameworkCore/HodHod.EntityFrameworkCore.csproj", "src/HodHod.EntityFrameworkCore/"]
COPY ["src/HodHod.Core/HodHod.Core.csproj", "src/HodHod.Core/"]
COPY ["src/HodHod.Core.Shared/HodHod.Core.Shared.csproj", "src/HodHod.Core.Shared/"]
RUN dotnet restore "src/HodHod.Migrator/HodHod.Migrator.csproj"
COPY . .
WORKDIR "/src/src/HodHod.Migrator"
RUN dotnet build "HodHod.Migrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HodHod.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HodHod.Migrator.dll", "-s"]
