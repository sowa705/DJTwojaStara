FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as runtime
WORKDIR /app
RUN apk add opus-dev libsodium ffmpeg yt-dlp
RUN cp /usr/lib/libopus.so.0 /app/opus.so && cp /usr/lib/libsodium.so.23 /app/libsodium.so
COPY --from=build /app/published-app /app

ENTRYPOINT ["dotnet", "DJTwojaStara.dll"]

EXPOSE 80