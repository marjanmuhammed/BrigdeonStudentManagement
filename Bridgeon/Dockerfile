# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Bridgeon/Bridgeon.csproj", "Bridgeon/"]
RUN dotnet restore "Bridgeon/Bridgeon.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR "/src/Bridgeon"
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Bridgeon.dll"]
