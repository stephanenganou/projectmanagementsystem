# Get base SDK Image from Microsoft
#Generate runtime Image
FROM mcr.microsoft.com/dotnet/framework/sdk:3.5
WORKDIR /app
EXPOSE 44304
COPY ./bin/app.publish/ .
WORKDIR ./bin
ENTRYPOINT ["dotnet", "PMSystem.dll"]