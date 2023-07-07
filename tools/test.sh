#!/usr/bin/env bash

# https://github.com/aws/aws-lambda-runtime-interface-emulator/releases/latest/download/aws-lambda-rie
docker run --rm -i -p 8080:8080 --name test -v "$(pwd)/bin:/mnt" --entrypoint "/mnt/alrie" test/proof-generator:latest-native "./ProofGenerator" >/dev/null 2>&1 &
sleep 1
echo "expected: 303, received: $(curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event_generate.json | jq '.statusCode')"
echo "expected: 200, received: $(curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event_download.json | jq '.statusCode')"
docker stop test >/dev/null