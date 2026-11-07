FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore the API and its shared Models project before copying source files.
COPY Models/Models.csproj Models/
COPY WebExportAPI/WebExportAPI.csproj WebExportAPI/
RUN dotnet restore WebExportAPI/WebExportAPI.csproj

COPY Models/ Models/
COPY WebExportAPI/ WebExportAPI/
RUN dotnet publish WebExportAPI/WebExportAPI.csproj -c Release --no-restore -o /out -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
COPY --from=build /out/ ./
USER $APP_UID
ENTRYPOINT ["dotnet", "WebExportAPI.dll"]
