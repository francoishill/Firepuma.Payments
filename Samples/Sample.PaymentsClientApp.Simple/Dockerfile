FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Samples/Sample.PaymentsClientApp.Simple/Sample.PaymentsClientApp.Simple.csproj", "Sample.PaymentsClientApp.Simple/"]
RUN dotnet restore "Samples/Sample.PaymentsClientApp.Simple/Sample.PaymentsClientApp.Simple.csproj"
COPY . .
WORKDIR "/src/Sample.PaymentsClientApp.Simple"
RUN dotnet build "Sample.PaymentsClientApp.Simple.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sample.PaymentsClientApp.Simple.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sample.PaymentsClientApp.Simple.dll"]
