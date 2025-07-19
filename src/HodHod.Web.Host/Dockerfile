# Use official ASP.NET 9.0 runtime as base image
FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use official .NET SDK 9.0 for building the app
FROM microsoft.repo.stinascloud.ir/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/HodHod.Web.Host/HodHod.Web.Host.csproj", "src/HodHod.Web.Host/"]
COPY ["src/HodHod.Web.Core/HodHod.Web.Core.csproj", "src/HodHod.Web.Core/"]
COPY ["src/HodHod.Application/HodHod.Application.csproj", "src/HodHod.Application/"]
COPY ["src/HodHod.Application.Shared/HodHod.Application.Shared.csproj", "src/HodHod.Application.Shared/"]
COPY ["src/HodHod.Core.Shared/HodHod.Core.Shared.csproj", "src/HodHod.Core.Shared/"]
COPY ["src/HodHod.Core/HodHod.Core.csproj", "src/HodHod.Core/"]
COPY ["src/HodHod.EntityFrameworkCore/HodHod.EntityFrameworkCore.csproj", "src/HodHod.EntityFrameworkCore/"]
COPY ["src/HodHod.GraphQL/HodHod.GraphQL.csproj", "src/HodHod.GraphQL/"]
COPY ["src/Abp.AspNetZeroCore/Abp.AspNetZeroCore.csproj", "src/Abp.AspNetZeroCore/"]


# Restore dependencies from custom NuGet source (Nexus)
RUN dotnet restore "src/HodHod.Web.Host/HodHod.Web.Host.csproj" \
    -s "https://nexus.repo.stinascloud.ir/repository/nuget/index.json"    

# Copy everything else
COPY . .

# Build the app
WORKDIR "/src/src/HodHod.Web.Host"
RUN dotnet build "HodHod.Web.Host.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "HodHod.Web.Host.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HodHod.Web.Host.dll"]
