#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft.repo.stinascloud.ir/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/HodHod.Web.Mvc/HodHod.Web.Mvc.csproj", "src/HodHod.Web.Mvc/"]
COPY ["src/HodHod.Web.Core/HodHod.Web.Core.csproj", "src/HodHod.Web.Core/"]
COPY ["src/HodHod.Application/HodHod.Application.csproj", "src/HodHod.Application/"]
COPY ["src/HodHod.Application.Shared/HodHod.Application.Shared.csproj", "src/HodHod.Application.Shared/"]
COPY ["src/HodHod.Core.Shared/HodHod.Core.Shared.csproj", "src/HodHod.Core.Shared/"]
COPY ["src/HodHod.Core/HodHod.Core.csproj", "src/HodHod.Core/"]
COPY ["src/HodHod.EntityFrameworkCore/HodHod.EntityFrameworkCore.csproj", "src/HodHod.EntityFrameworkCore/"]
COPY ["src/HodHod.GraphQL/HodHod.GraphQL.csproj", "src/HodHod.GraphQL/"]
RUN dotnet restore "src/HodHod.Web.Mvc/HodHod.Web.Mvc.csproj"
COPY . .
WORKDIR "/src/src/HodHod.Web.Mvc"
RUN dotnet build "HodHod.Web.Mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HodHod.Web.Mvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HodHod.Web.Mvc.dll"]
