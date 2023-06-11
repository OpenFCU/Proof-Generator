#!/usr/bin/env bash

docker run --rm -dq -p 8080:8080 -m 256m --name proof-generator test/proof-generator 1>&2
curl -kSsL "http://localhost:8080/2015-03-31/functions/function/invocations" --json @tools/event.json
docker stop proof-generator 1>&2