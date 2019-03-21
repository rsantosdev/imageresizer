FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
RUN apt-get update \
    && apt-get install -y imagemagick \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["imageresizer.csproj", "./"]
RUN dotnet restore "./imageresizer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "imageresizer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "imageresizer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "imageresizer.dll"]
