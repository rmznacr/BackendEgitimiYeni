using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendEgitimiYeni.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunkHnswIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX "IX_DocumentChunks_Embedding_Hnsw"
                ON "DocumentChunks"
                USING hnsw ("Embedding" vector_cosine_ops);
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_DocumentChunks_Embedding_Hnsw";
                """
            );
        }
    }
}