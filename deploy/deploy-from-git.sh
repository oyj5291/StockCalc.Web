#!/usr/bin/env bash
set -euo pipefail

APP_NAME="stockcalc"
APP_DLL="StockCalc.Web.dll"
REPO_DIR="/opt/stockcalc-src"
PROJECT_PATH="$REPO_DIR/StockCalc.Web/StockCalc.Web.csproj"
PUBLISH_DIR="$REPO_DIR/artifacts/publish"
APP_DIR="/var/www/stockcalc"

cd "$REPO_DIR"
git pull origin main
dotnet publish "$PROJECT_PATH" -c Release -o "$PUBLISH_DIR"

sudo systemctl stop "$APP_NAME" || true
sudo mkdir -p "$APP_DIR"
sudo rsync -av --delete "$PUBLISH_DIR/" "$APP_DIR/"
sudo chown -R www-data:www-data "$APP_DIR"
sudo chmod -R u=rwX,g=rX,o= "$APP_DIR"

test -f "$APP_DIR/$APP_DLL"

sudo systemctl start "$APP_NAME"
sudo systemctl status "$APP_NAME" --no-pager
curl --fail http://127.0.0.1:5001/healthz
