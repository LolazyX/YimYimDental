using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YimYimDental.Migrations
{
    /// <inheritdoc />
    public partial class updateTreatmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "TreatmentHistories");

            migrationBuilder.RenameColumn(
                name: "Symptoms",
                table: "TreatmentHistories",
                newName: "TreatmentRights");

            migrationBuilder.AddColumn<string>(
                name: "EquipmentDetails",
                table: "TreatmentHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentDetails",
                table: "TreatmentHistories");

            migrationBuilder.RenameColumn(
                name: "TreatmentRights",
                table: "TreatmentHistories",
                newName: "Symptoms");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "TreatmentHistories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
