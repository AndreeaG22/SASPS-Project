-- Create schemas
CREATE SCHEMA IF NOT EXISTS document;
CREATE SCHEMA IF NOT EXISTS versioning;
CREATE SCHEMA IF NOT EXISTS tagging;
CREATE SCHEMA IF NOT EXISTS metadata_indexing;

-- Grant permissions to the user
GRANT ALL PRIVILEGES ON SCHEMA document TO docustore_ar;
GRANT ALL PRIVILEGES ON SCHEMA versioning TO docustore_ar;
GRANT ALL PRIVILEGES ON SCHEMA tagging TO docustore_ar;
GRANT ALL PRIVILEGES ON SCHEMA metadata_indexing TO docustore_ar;

-- Grant usage and create on each schema
GRANT USAGE, CREATE ON SCHEMA document TO docustore_ar;
GRANT USAGE, CREATE ON SCHEMA versioning TO docustore_ar;
GRANT USAGE, CREATE ON SCHEMA tagging TO docustore_ar;
GRANT USAGE, CREATE ON SCHEMA metadata_indexing TO docustore_ar;
