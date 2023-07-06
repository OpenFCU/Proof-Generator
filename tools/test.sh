#!/usr/bin/env bash

# https://github.com/aws/aws-lambda-runtime-interface-emulator/releases/latest/download/aws-lambda-rie
docker run --rm -i -p 8080:8080 --name test -v "$(pwd)/bin:/mnt" --entrypoint "/mnt/alrie" test/proof-generator:latest-native "./ProofGenerator" &
sleep 2
curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event.json
docker stop test