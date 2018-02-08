#!/bin/bash

set -e

cd Source/Graceterm

ARTIFACTS_FOLDER=./artifacts

if [ ! -d $ARTIFACTS_FOLDER ]
then
	mkdir $ARTIFACTS_FOLDER
fi

if [ ${TRAVIS_TAG^^} = *"BETA"* ]
then
	SUFFIX_ARG="--version-suffix=beta-b$TRAVIS_BUILD_NUMBER"
elif [ ${TRAVIS_TAG^^} = *"ALPHA"* ]
then
	SUFFIX_ARG="--version-suffix=alpha-b$TRAVIS_BUILD_NUMBER"
fi

dotnet pack -c Release $SUFFIX_ARG -o $ARTIFACTS_FOLDER
dotnet nuget push --api-key $NUGET_API_KEY $ARTIFACTS_FOLDER/*.nupkg --source https://api.nuget.org/v3/index.json

rm -Rf $ARTIFACTS_FOLDER
