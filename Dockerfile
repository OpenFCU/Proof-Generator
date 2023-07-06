FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

ARG TARGETARCH

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source

RUN apk add clang build-base zlib-dev
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet build

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM alpine:3.18
LABEL maintainer="mikucat0309 <admin@mikuc.at>"

WORKDIR /app
RUN apk add --no-cache tzdata fontconfig libstdc++
COPY --from=publish /app .

ENV TZ=Asia/Taipei
ENTRYPOINT [ "./ProofGenerator" ]
