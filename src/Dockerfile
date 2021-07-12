#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster AS build
WORKDIR /src
COPY ["TG.Auth.Api/TG.Auth.Api.csproj", "TG.Auth.Api/"]
COPY nuget.config .
RUN dotnet restore "TG.Auth.Api/TG.Auth.Api.csproj"
COPY . .
WORKDIR "/src/TG.Auth.Api"
RUN dotnet build "TG.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TG.Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TG.Auth.Api.dll"]