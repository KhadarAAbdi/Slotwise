# Stage 1: build. The SDK image has the compiler and is large, so it is only used to produce the published output.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the .csproj files first and restore packages. Docker caches each step, so as long as the
# project files don't change, a code-only change skips the slow restore on the next build.
COPY Slotwise.Domain/Slotwise.Domain.csproj Slotwise.Domain/
COPY Slotwise.Application/Slotwise.Application.csproj Slotwise.Application/
COPY Slotwise.Infrastructure/Slotwise.Infrastructure.csproj Slotwise.Infrastructure/
COPY Slotwise.API/Slotwise.API.csproj Slotwise.API/
RUN dotnet restore Slotwise.API/Slotwise.API.csproj

# Now copy the source and publish. Tests and the web frontend are not needed to run the API.
COPY Slotwise.Domain/ Slotwise.Domain/
COPY Slotwise.Application/ Slotwise.Application/
COPY Slotwise.Infrastructure/ Slotwise.Infrastructure/
COPY Slotwise.API/ Slotwise.API/
RUN dotnet publish Slotwise.API/Slotwise.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: run. The ASP.NET runtime image is much smaller because it has no compiler.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The official image listens on 8080 and ships a non-root user, so don't run as root.
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Slotwise.API.dll"]
