using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus_BAVer2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cardanoindexer");

            migrationBuilder.CreateTable(
                name: "BalanceByAddress",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Address = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceByAddress", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "InputsBySlot",
                schema: "cardanoindexer",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    OutputIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputsBySlot", x => new { x.TxHash, x.OutputIndex });
                });

            migrationBuilder.CreateTable(
                name: "OutputsBySlot",
                schema: "cardanoindexer",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputsBySlot", x => new { x.TxHash, x.TxIndex });
                });

            migrationBuilder.CreateTable(
                name: "ReducerStates",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReducerStates", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceByAddress",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "InputsBySlot",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "OutputsBySlot",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "ReducerStates",
                schema: "cardanoindexer");
        }
    }
}
