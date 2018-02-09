#!/bin/bash

set -e

cd Source/Graceterm

ARTIFACTS_FOLDER=./artifacts

if [ ! -d $ARTIFACTS_FOLDER ]
then
	mkdir $ARTIFACTS_FOLDER
fi

echo "**** VARIABLES"
echo "TRAVIS_BRANCH: $TRAVIS_BRANCH"
echo "TRAVIS_TAG: $TRAVIS_TAG"


if [ ${TRAVIS_BRANCH^^} = *"ALPHA"* ] || [ ${TRAVIS_BRANCH^^} = *"BETA"* ]
then
	SUFFIX_ARG="--version-suffix=p$TRAVIS_BRANCH-b$TRAVIS_BUILD_NUMBER"
fi

dotnet pack -c Release $SUFFIX_ARG -o $ARTIFACTS_FOLDER
#dotnet nuget push --api-key $NUGET_API_KEY $ARTIFACTS_FOLDER/*.nupkg --source https://api.nuget.org/v3/index.json

rm -Rf $ARTIFACTS_FOLDER
