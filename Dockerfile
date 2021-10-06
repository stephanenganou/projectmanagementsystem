# Get base SDK Image from Microsoft
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

#Copy the CSPROJ file and restore and dependencies (via NUGET)
COPY *.csproj ./
RUN dotnet restore

#Copy the project files and build our realease
COPY . ./
RUN dotnet publish "mvc-sample-app.csproj" -c Release -o publish

#Generate runtime Image
FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app
EXPOSE 44304
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "PMSystem.dll"]