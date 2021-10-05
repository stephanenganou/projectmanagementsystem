# Get base SDK Image from Microsoft
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

#Copy the CSPROJ file and restore and dependencies (via NUGET)
COPY *.csproj ./
RUN dotnet restore

#Copy the project files and build our realease
COPY . ./
RUN dotnet publish --configuration Release --output out

#Generate runtime Image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
EXPOSE 8001
COPY --from=build-env /app/out ./
ENTRYPOINT ["dotnet", "PMSystem.dll"]