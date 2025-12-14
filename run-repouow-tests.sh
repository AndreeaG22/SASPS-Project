#!/bin/bash

# Repository/UoW Test Runner

echo "Starting Repository/UoW tests..."
echo ""

# Run all test projects
dotnet test docustore-repoUow/tests/Document.Domain.Tests/Document.Domain.Tests.csproj
dotnet test docustore-repoUow/tests/Document.Application.Tests/Document.Application.Tests.csproj
dotnet test docustore-repoUow/tests/Versioning.Domain.Tests/Versioning.Domain.Tests.csproj
dotnet test docustore-repoUow/tests/Tagging.Domain.Tests/Tagging.Domain.Tests.csproj
dotnet test docustore-repoUow/tests/Document.Infrastructure.Tests/Document.Infrastructure.Tests.csproj
dotnet test docustore-repoUow/tests/MetadataIndexing.Domain.Tests/MetadataIndexing.Domain.Tests.csproj

echo ""
echo "Done."