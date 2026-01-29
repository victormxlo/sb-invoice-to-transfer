# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY SB.InvoiceToTransfer.sln .

COPY src/SB.InvoiceToTransfer.Api/SB.InvoiceToTransfer.Api.csproj src/SB.InvoiceToTransfer.Api/
COPY src/SB.InvoiceToTransfer.Application/SB.InvoiceToTransfer.Application.csproj src/SB.InvoiceToTransfer.Application/
COPY src/SB.InvoiceToTransfer.Domain/SB.InvoiceToTransfer.Domain.csproj src/SB.InvoiceToTransfer.Domain/
COPY src/SB.InvoiceToTransfer.Infrastructure/SB.InvoiceToTransfer.Infrastructure.csproj src/SB.InvoiceToTransfer.Infrastructure/

RUN dotnet restore

COPY src/ src/

RUN dotnet publish src/SB.InvoiceToTransfer.Api/SB.InvoiceToTransfer.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SB.InvoiceToTransfer.Api.dll"]
