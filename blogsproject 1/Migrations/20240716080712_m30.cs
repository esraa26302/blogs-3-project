using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogsproject_1.Migrations
{
    /// <inheritdoc />
    public partial class m30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OneId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OneId",
                table: "Users");
        }
    }
}
