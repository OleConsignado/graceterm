#!/bin/bash

set -e

cd Source/Graceterm

ARTIFACTS_FOLDER=./artifacts

if [ ! -d $ARTIFACTS_FOLDER ]
then
	mkdir $ARTIFACTS_FOLDER
fi

if [[ ${TRAVIS_BRANCH^^} = *"ALPHA"* ]] || [[ ${TRAVIS_BRANCH^^} = *"BETA"* ]]
then
	SUFFIX_ARG="--version-suffix=p$TRAVIS_BRANCH-b$TRAVIS_BUILD_NUMBER"
fi

echo "SUFFIX_ARG: $SUFFIX_ARG"
echo "TRAVIS_BRANCH: $TRAVIS_BRANCH"
echo "TRAVIS_BRANCH^^: ${TRAVIS_BRANCH^^}"

dotnet pack -c Release $SUFFIX_ARG -o $ARTIFACTS_FOLDER
#dotnet nuget push --api-key $NUGET_API_KEY $ARTIFACTS_FOLDER/*.nupkg --source https://api.nuget.org/v3/index.json

rm -Rf $ARTIFACTS_FOLDER
