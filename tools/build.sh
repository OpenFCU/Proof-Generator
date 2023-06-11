#!/usr/bin/env bash
set -e

docker buildx build . -t test/proof-generator:latest --load
