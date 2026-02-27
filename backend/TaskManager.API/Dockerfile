FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManager.sln ./
COPY TaskManager.API/TaskManager.API.csproj TaskManager.API/
COPY TaskManager.Application/TaskManager.Application.csproj TaskManager.Application/
COPY TaskManager.Domain/TaskManager.Domain.csproj TaskManager.Domain/
COPY TaskManager.Infrastructure/TaskManager.Infrastructure.csproj TaskManager.Infrastructure/
RUN dotnet restore TaskManager.sln

COPY . .
RUN dotnet publish TaskManager.API/TaskManager.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Railway provides PORT env var, Program.cs will use it
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV PORT=8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]

