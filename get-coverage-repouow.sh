#!/bin/bash

echo "Generating coverage for Repository/UoW tests..."

# Run tests with coverage for each project
dotnet test docustore-repoUow/tests/Document.Domain.Tests/Document.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/Document.Domain
dotnet test docustore-repoUow/tests/Document.Application.Tests/Document.Application.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/Document.Application
dotnet test docustore-repoUow/tests/Document.Infrastructure.Tests/Document.Infrastructure.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/Document.Infrastructure
dotnet test docustore-repoUow/tests/Versioning.Domain.Tests/Versioning.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/Versioning.Domain
dotnet test docustore-repoUow/tests/Tagging.Domain.Tests/Tagging.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/Tagging.Domain
dotnet test docustore-repoUow/tests/MetadataIndexing.Domain.Tests/MetadataIndexing.Domain.Tests.csproj --collect:"XPlat Code Coverage" --results-directory docustore-repoUow/tests/coverage/MetadataIndexing.Domain

echo ""
echo "Coverage reports generated in: docustore-repoUow/tests/coverage/"
echo ""
echo "To generate HTML report, install ReportGenerator:"
echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"
echo ""
echo "Then run:"
echo "  reportgenerator -reports:\"docustore-repoUow/tests/coverage/**/coverage.cobertura.xml\" -targetdir:\"docustore-repoUow/tests/coverage/html\" -reporttypes:Html"
echo ""
echo "Open: docustore-repoUow/tests/coverage/html/index.html"