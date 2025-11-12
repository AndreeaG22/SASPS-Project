#!/bin/bash

echo "📦 Installing required NuGet packages for DocuStore..."
echo "========================================================"

PROJECT_ROOT="/home/ioan/RiderProjects/SASPS-Project/docustore-activerecord"
cd "$PROJECT_ROOT"

# Install Swagger/OpenAPI packages for Gateway
echo ""
echo "🔧 Installing Swagger for API Gateway..."
dotnet add src/DocuStore.Gateway/DocuStore.Gateway.csproj package Swashbuckle.AspNetCore

# Install Swagger for each module API
echo ""
echo "🔧 Installing Swagger for Document.API..."
dotnet add src/Document/Document.API/Document.API.csproj package Swashbuckle.AspNetCore

echo ""
echo "🔧 Installing Swagger for Versioning.API..."
dotnet add src/Versioning/Versioning.API/Versioning.API.csproj package Swashbuckle.AspNetCore

echo ""
echo "🔧 Installing Swagger for Tagging.API..."
dotnet add src/Tagging/Tagging.API/Tagging.API.csproj package Swashbuckle.AspNetCore

echo ""
echo "🔧 Installing Swagger for MetadataIndexing.API..."
dotnet add src/MetadataIndexing/MetadataIndexing.API/MetadataIndexing.API.csproj package Swashbuckle.AspNetCore

# Install Entity Framework Core (for future use)
echo ""
echo "🔧 Installing Entity Framework Core packages..."

# For each Infrastructure project
for module in "Document" "Versioning" "Tagging" "MetadataIndexing"
do
    echo "  ➜ Installing EF Core for $module.Infrastructure..."
    dotnet add "src/$module/$module.Infrastructure/$module.Infrastructure.csproj" package Microsoft.EntityFrameworkCore
    dotnet add "src/$module/$module.Infrastructure/$module.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.SqlServer
    dotnet add "src/$module/$module.Infrastructure/$module.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design
done

# Install common packages for Shared.Common
echo ""
echo "🔧 Installing packages for Shared.Common..."
dotnet add src/Shared/Shared.Common/Shared.Common.csproj package Microsoft.Extensions.DependencyInjection.Abstractions

echo ""
echo "✅ All packages installed successfully!"
echo ""
echo "Building solution to verify..."
dotnet build

echo ""
echo "✅ Setup complete! Ready to configure Swagger."