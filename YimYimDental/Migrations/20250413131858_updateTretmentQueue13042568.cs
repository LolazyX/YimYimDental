using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YimYimDental.Migrations
{
    /// <inheritdoc />
    public partial class updateTretmentQueue13042568 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreliminarySymptoms",
                table: "TreatmentQueues",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreliminarySymptoms",
                table: "TreatmentQueues");
        }
    }
}
