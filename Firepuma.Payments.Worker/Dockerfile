FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS restore
WORKDIR /src

COPY ["Firepuma.Payments.Domain/Firepuma.Payments.Domain.csproj", "Firepuma.Payments.Domain/"]
COPY ["Firepuma.Payments.Infrastructure/Firepuma.Payments.Infrastructure.csproj", "Firepuma.Payments.Infrastructure/"]
COPY ["Firepuma.Payments.Tests/Firepuma.Payments.Tests.csproj", "Firepuma.Payments.Tests/"]

COPY ["Firepuma.Payments.Worker/Firepuma.Payments.Worker.csproj", "Firepuma.Payments.Worker/"]
RUN dotnet restore "Firepuma.Payments.Worker/Firepuma.Payments.Worker.csproj"
COPY . .


FROM restore AS test
RUN dotnet restore "Firepuma.Payments.Tests/Firepuma.Payments.Tests.csproj"
RUN dotnet test --no-restore --logger:trx --results-directory /testsresults


FROM test AS publish
ARG target_configuration=Release
ARG version=dev-invalid-version
WORKDIR /src/Firepuma.Payments.Worker
RUN dotnet publish  "Firepuma.Payments.Worker.csproj" -c ${target_configuration} --no-restore /p:Version=${version} -o /app


FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
LABEL maintainer="Francois Hill <francoishill11@gmail.com>"
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "Firepuma.Payments.Worker.dll"]