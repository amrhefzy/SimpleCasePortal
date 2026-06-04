using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleCasePortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveViewerReportsViewPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 5 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 16, 5 });
        }
    }
}
