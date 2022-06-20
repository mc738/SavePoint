FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SavePoint/SavePoint.fsproj", "SavePoint/"]
RUN dotnet restore "SavePoint/SavePoint.fsproj"
COPY . .
WORKDIR "/src/SavePoint"
RUN dotnet build "SavePoint.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SavePoint.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SavePoint.dll"]
