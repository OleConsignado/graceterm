#!/bin/bash

set -e

cd Source/Graceterm

dotnet restore
dotnet build
