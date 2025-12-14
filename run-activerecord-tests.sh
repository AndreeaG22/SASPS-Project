#!/bin/bash

# Simple Active Record Test Runner

echo "Starting Active Record tests..."
echo ""

# Check Docker
if ! docker info > /dev/null 2>&1; then
    echo "Error: Docker not running"
    exit 1
fi

# Start database if needed
if ! docker ps | grep -q "docustore-ar-db"; then
    echo "Starting database..."
    docker-compose up -d postgres-activerecord
    sleep 5
fi

# Run tests
dotnet test docustore-activerecord/tests/Document.Domain.Tests/Document.Domain.Tests.csproj
dotnet test docustore-activerecord/tests/Versioning.Domain.Tests/Versioning.Domain.Tests.csproj
dotnet test docustore-activerecord/tests/Tagging.Domain.Tests/Tagging.Domain.Tests.csproj
dotnet test docustore-activerecord/tests/MetadataIndexing.Domain.Tests/MetadataIndexing.Domain.Tests.csproj

echo ""
echo "Done. To stop database: docker-compose stop postgres-activerecord"