FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source

RUN apk add clang build-base zlib-dev
COPY *.csproj .
RUN dotnet restore
COPY . .

FROM build AS publish
RUN dotnet publish -c Release -o /app -p PublishAot=false

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
LABEL maintainer="mikucat0309 <admin@mikuc.at>"

WORKDIR /app
RUN apk add --no-cache tzdata fontconfig
COPY --from=publish /app .

ENV TZ=Asia/Taipei
ENTRYPOINT [ "./ProofGenerator" ]
