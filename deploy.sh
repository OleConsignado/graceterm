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
	SUFFIX=$(echo $TRAVIS_BRANCH-build$TRAVIS_BUILD_NUMBER | sed 's/[^0-9A-Za-z-]//g')
	SUFFIX_ARG="--version-suffix=$SUFFIX"
fi

dotnet pack -c Release $SUFFIX_ARG -o $ARTIFACTS_FOLDER
dotnet nuget push --api-key $NUGET_API_KEY $ARTIFACTS_FOLDER/*.nupkg --source https://api.nuget.org/v3/index.json

rm -Rf $ARTIFACTS_FOLDER
