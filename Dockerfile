FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source

RUN apk add clang build-base zlib-dev
COPY *.csproj .
RUN dotnet restore -r linux-x64 --locked-mode

FROM build AS publish
RUN dotnet publish -r linux-x64 -c Release -o /app

FROM alpine:3.18
LABEL maintainer="mikucat0309 <admin@mikuc.at>"

WORKDIR /app
RUN apk add --no-cache tzdata fontconfig libstdc++
COPY --from=publish /app .

ENV TZ=Asia/Taipei
ENTRYPOINT [ "./ProofGenerator" ]
