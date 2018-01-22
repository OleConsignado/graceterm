#!/bin/bash

set -e

cd Source/Graceterm

ARTIFACTS_FOLDER=./artifacts

if [ ! -d $ARTIFACTS_FOLDER ]
then
	mkdir $ARTIFACTS_FOLDER
fi

dotnet pack -c Release --version-suffix=alpha-b$TRAVIS_BUILD_NUMBER -o $ARTIFACTS_FOLDER
dotnet nuget push --api-key $NUGET_API_KEY $ARTIFACTS_FOLDER/*.nupkg --source https://api.nuget.org/v3/index.json

rm -Rf $ARTIFACTS_FOLDER
