#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR"

./scripts/setup-node.sh

NODE_DIR="$SCRIPT_DIR/.node/current"
export PATH="$NODE_DIR/bin:$PATH"

if [ ! -d "$SCRIPT_DIR/node_modules" ]; then
  npm install
fi

npm start
