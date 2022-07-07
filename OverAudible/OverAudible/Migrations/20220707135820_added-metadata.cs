using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverAudible.Migrations
{
    /// <inheritdoc />
    public partial class addedmetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentMetadataJson",
                table: "OfflineLibrary",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentMetadataJson",
                table: "OfflineLibrary");
        }
    }
}
