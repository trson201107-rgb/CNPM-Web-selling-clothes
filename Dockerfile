# ---- Giai đoạn build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ThoiTrangNam.Web/ThoiTrangNam.Web.csproj src/ThoiTrangNam.Web/
RUN dotnet restore src/ThoiTrangNam.Web/ThoiTrangNam.Web.csproj
COPY src/ src/
RUN dotnet publish src/ThoiTrangNam.Web/ThoiTrangNam.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Giai đoạn chạy ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ThoiTrangNam.Web.dll"]
