#!/bin/bash
dotnet test AuthServer.sln --collect:"XPlat Code Coverage"

# Generate HTML coverage report
export PATH="$PATH:/Users/youchen/.dotnet/tools"
reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/html" -reporttypes:"Html"

echo "Coverage report generated at: TestResults/html/index.html"