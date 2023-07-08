#!/usr/bin/env bash
set -e

docker buildx build . -f CLR.dockerfile -t test/proof-generator:latest --load
