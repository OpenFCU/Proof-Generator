#!/usr/bin/env bash
set -e

docker run -d --rm -p 8080:8080 --name proof-generator test/proof-generator >/dev/null
curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" -d @tools/event.json
docker stop proof-generator >/dev/null
echo