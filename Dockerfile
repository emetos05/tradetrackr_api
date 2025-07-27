# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY tradetrackr.api.csproj .
RUN dotnet restore "tradetrackr.api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "tradetrackr.api.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "tradetrackr.api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the working directory
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Expose the port that Render uses
EXPOSE 10000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Define the entry point
ENTRYPOINT ["dotnet", "tradetrackr.api.dll"]