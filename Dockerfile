FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

ARG TARGETARCH

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source

COPY *.csproj .
RUN dotnet restore -a $TARGETARCH
COPY . .
RUN dotnet build -a $TARGETARCH --no-restore

FROM build AS publish
RUN dotnet publish -a $TARGETARCH --no-restore -c Release --self-contained false -o /app

FROM docker.io/mikucat0309/lambda-aspnet:7.0-alpine
LABEL maintainer="mikucat0309 <admin@mikuc.at>"

RUN apk add --no-cache tzdata fontconfig
COPY --from=publish /app .

ENV TZ=Asia/Taipei
CMD [ "ProofGenerator" ]
