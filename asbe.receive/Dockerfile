#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["asbe.receive/asbe.receive.csproj", "asbe.receive/"]
COPY ["asbe.send/asbe.send.csproj", "asbe.send/"]
RUN dotnet restore "asbe.receive/asbe.receive.csproj"
COPY . .
WORKDIR "/src/asbe.receive"
RUN dotnet build "asbe.receive.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "asbe.receive.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "asbe.receive.dll"]