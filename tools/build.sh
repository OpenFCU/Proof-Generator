#!/usr/bin/env bash
set -e

docker buildx build . -f Dockerfile -t test/proof-generator:latest-native --load
