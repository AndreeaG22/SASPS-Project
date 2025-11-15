FROM mcr.microsoft.com/dotnet/sdk:10.0

# Set working directory
WORKDIR /src

# Install dotnet watch tool globally
RUN dotnet tool install -g Microsoft.DotNet.Watch.Tools

# Add dotnet tools to PATH so we can use them
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy the entire project to container
# This happens ONCE when container starts
COPY . .

# Expose the port (same as your local port)
EXPOSE 5021

# Default command (can be overridden in docker-compose)
CMD ["dotnet", "watch", "run"]
