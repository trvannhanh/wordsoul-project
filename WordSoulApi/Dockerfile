# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution + project
COPY WordSoulApi.sln ./
COPY WordSoulApi/*.csproj ./WordSoulApi/

RUN dotnet restore WordSoulApi/WordSoulApi.csproj

# copy toàn bộ source
COPY . .
WORKDIR /src/WordSoulApi
RUN dotnet publish -c Release -o /app

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "WordSoulApi.dll"]
