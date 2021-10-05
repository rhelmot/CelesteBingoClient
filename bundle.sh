#!/bin/bash -e

dotnet build
VERSION=$(cat BingoClient/everest.yaml | grep '^  Version' | cut -d' ' -f 4)
FILENAME=dist/BingoClient_${VERSION}${2}.zip
rm -f $FILENAME
cd BingoClient/bin/${1-Debug}
zip -r ../../../${FILENAME} *
echo Finished in ${FILENAME}
