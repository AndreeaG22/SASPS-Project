using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetadataIndexing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "metadata_indexing");

            migrationBuilder.CreateTable(
                name: "SearchDocumentIndexes",
                schema: "metadata_indexing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocumentIndexes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentIndexes_CreatedAt",
                schema: "metadata_indexing",
                table: "SearchDocumentIndexes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentIndexes_CreatedBy",
                schema: "metadata_indexing",
                table: "SearchDocumentIndexes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentIndexes_DocumentId_Unique",
                schema: "metadata_indexing",
                table: "SearchDocumentIndexes",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentIndexes_IsDeleted",
                schema: "metadata_indexing",
                table: "SearchDocumentIndexes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentIndexes_Title",
                schema: "metadata_indexing",
                table: "SearchDocumentIndexes",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchDocumentIndexes",
                schema: "metadata_indexing");
        }
    }
}
