#!/usr/bin/env bash

# https://github.com/aws/aws-lambda-runtime-interface-emulator/releases/latest/download/aws-lambda-rie
docker run --rm -i -p 8080:8080 --name test -v "$(pwd)/bin:/mnt" --entrypoint "/mnt/alrie" test/proof-generator:latest "./ProofGenerator" >/dev/null 2>&1 &
sleep 1
body=$(jq -cM '.' tools/example.json)
jq -cM '. | .requestContext.http.method = "POST" | .body = $ARGS.named["body"]' tools/event_template.json --arg body "$body" > tools/event_generate.json
result=$(curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event_generate.json)
echo $result
echo "expected: 200, received: $(jq '.statusCode' <<<"$result")"
result=$(jq -r '.body' <<<"$result")
filepath=$(jq -r '.path' <<<"$result")
echo "generated path: $filepath"
jq -cM '. | .rawPath = $ARGS.named["path"] | .requestContext.http.path = $ARGS.named["path"]' tools/event_template.json --arg path ${filepath#.} > tools/event_download.json
result=$(curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event_download.json)
docker stop test >/dev/null
echo "expected: 200, received: $(jq '.statusCode' <<<"$result")"
jq -r '.body' <<<"$result" | base64 -d > cache/example.webp
rm tools/event_generate.json tools/event_download.json
