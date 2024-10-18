using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus_BAVer2.Migrations
{
    /// <inheritdoc />
    public partial class NameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutputIndex",
                schema: "cardanoindexer",
                table: "InputsBySlot",
                newName: "TxIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TxIndex",
                schema: "cardanoindexer",
                table: "InputsBySlot",
                newName: "OutputIndex");
        }
    }
}
