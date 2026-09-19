FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FairPlay.Sports.Domain/FairPlay.Sports.Domain.csproj src/FairPlay.Sports.Domain/
COPY src/FairPlay.Sports.Application/FairPlay.Sports.Application.csproj src/FairPlay.Sports.Application/
COPY src/FairPlay.Sports.Infrastructure/FairPlay.Sports.Infrastructure.csproj src/FairPlay.Sports.Infrastructure/
COPY src/FairPlay.Sports.Api/FairPlay.Sports.Api.csproj src/FairPlay.Sports.Api/
RUN dotnet restore src/FairPlay.Sports.Api/FairPlay.Sports.Api.csproj

COPY src/ src/
RUN dotnet publish src/FairPlay.Sports.Api/FairPlay.Sports.Api.csproj -c Release -o /app --no-restore

# A self-contained migrations bundle, so the Production image (aspnet runtime only, no SDK)
# can still apply migrations from a Pre-Deploy Command without `dotnet ef` being installed there.
RUN dotnet tool install --global dotnet-ef --version 10.0.11
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
      -p src/FairPlay.Sports.Infrastructure \
      -s src/FairPlay.Sports.Api \
      -o /app/efbundle \
      --self-contained -r linux-x64 \
      --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Render injects PORT at container start; fall back to 8080 for local `docker run`.
EXPOSE 8080
ENTRYPOINT dotnet FairPlay.Sports.Api.dll --urls http://0.0.0.0:${PORT:-8080}
