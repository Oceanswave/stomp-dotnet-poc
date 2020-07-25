#!/bin/sh
set -ea

if [ ! -f "package.json" ]; then

    echo "package.json not found"
    
fi

echo "Ensuring dependencies..."
yarn install

echo "Starting your app..."

exec "$@"