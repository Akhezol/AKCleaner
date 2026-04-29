#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
UI_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

NODE_VERSION="${NODE_VERSION:-20.11.1}"
NODE_DIR="$UI_ROOT/.node"
CURRENT_LINK="$NODE_DIR/current"
TARBALL_DIR="$NODE_DIR/tarballs"

log() { printf '%s\n' "$*"; }

have_node() { command -v node >/dev/null 2>&1; }
have_npm() { command -v npm >/dev/null 2>&1; }

if have_node && have_npm; then
  log "node/npm zaten var. Devam ediliyor..."
  exit 0
fi

install_with_apt() {
  if command -v apt-get >/dev/null 2>&1 && command -v sudo >/dev/null 2>&1; then
    log "Debian/Ubuntu apt-get ile node/npm kurulumu deneniyor..."
    sudo apt-get update || return 1
    sudo apt-get install -y nodejs npm || return 1
    exit 0
  fi
  return 1
}

if install_with_apt; then
  exit 0
fi

mkdir -p "$TARBALL_DIR" "$NODE_DIR"

platform="linux-x64"
tarball="$TARBALL_DIR/node-v$NODE_VERSION-$platform.tar.xz"
url="https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-$platform.tar.xz"

if [ ! -f "$tarball" ]; then
  log "Portable Node indiriliyor: $url"
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$url" -o "$tarball"
  else
    # wget fallback
    wget -qO "$tarball" "$url"
  fi
fi

tmp_extract="$NODE_DIR/_extract"
rm -rf "$tmp_extract"
mkdir -p "$tmp_extract"

log "Portable Node çıkarılıyor..."
tar -xJf "$tarball" -C "$tmp_extract"

extracted_dir=""
for d in "$tmp_extract"/node-v$NODE_VERSION-$platform; do
  if [ -d "$d" ]; then
    extracted_dir="$d"
    break
  fi
done

if [ -z "$extracted_dir" ]; then
  for d in "$tmp_extract"/node-v*-*; do
    if [ -d "$d" ]; then
      extracted_dir="$d"
      break
    fi
  done
fi

if [ -z "$extracted_dir" ]; then
  log "Portable Node çıkarımı başarısız."
  exit 1
fi

rm -rf "$CURRENT_LINK" || true
ln -s "$extracted_dir" "$CURRENT_LINK"

log "Portable Node hazır: $(readlink -f "$CURRENT_LINK" 2>/dev/null || true)"
log "PATH günlemek için dev.sh kullan: ./dev.sh"
