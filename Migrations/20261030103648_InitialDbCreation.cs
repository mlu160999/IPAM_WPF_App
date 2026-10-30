using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPAM_WPF_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subnet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", nullable: false),
                    Erstellt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subnet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPAdresse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubnetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPAdresse", x => x.Id);
                    table.CheckConstraint("CK_IPAdresse_Status", "\"Status\" IN (0, 1, 2)");
                    table.ForeignKey(
                        name: "FK_IPAdresse_Subnet_SubnetId",
                        column: x => x.SubnetId,
                        principalTable: "Subnet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IPAdresse_SubnetId_Adresse",
                table: "IPAdresse",
                columns: new[] { "SubnetId", "Adresse" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IPAdresse");

            migrationBuilder.DropTable(
                name: "Subnet");
        }
    }
}
