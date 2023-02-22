FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Shell/Shell.csproj", "Shell/"]
RUN dotnet restore "Shell/Shell.csproj"
COPY . .
WORKDIR "/src/Shell"
RUN dotnet build "Shell.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shell.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shell.dll"]
