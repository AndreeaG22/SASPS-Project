#!/bin/bash

echo "Generating coverage for Active Record tests..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Docker is not running. Please start Docker."
    exit 1
fi

# Start PostgreSQL if not running
echo "Starting PostgreSQL database..."
docker-compose up -d postgres-activerecord > /dev/null 2>&1
sleep 2

# Run tests with coverage for each project
dotnet test docustore-activerecord/tests/Document.Domain.Tests/Document.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-activerecord/tests/coverage/Document.Domain
dotnet test docustore-activerecord/tests/Versioning.Domain.Tests/Versioning.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-activerecord/tests/coverage/Versioning.Domain
dotnet test docustore-activerecord/tests/Tagging.Domain.Tests/Tagging.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-activerecord/tests/coverage/Tagging.Domain
dotnet test docustore-activerecord/tests/MetadataIndexing.Domain.Tests/MetadataIndexing.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-activerecord/tests/coverage/MetadataIndexing.Domain

echo ""
echo "Coverage reports generated in: docustore-activerecord/tests/coverage/"
echo ""
echo "To generate HTML report, install ReportGenerator:"
echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"
echo ""
echo "Then run:"
echo "  reportgenerator -reports:\"docustore-activerecord/tests/coverage/**/coverage.cobertura.xml\" -targetdir:\"docustore-activerecord/tests/coverage/html\" -reporttypes:Html"
echo ""
echo "Open: docustore-activerecord/tests/coverage/html/index.html"