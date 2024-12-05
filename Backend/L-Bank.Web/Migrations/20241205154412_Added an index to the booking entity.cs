using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L_Bank_W_Backend.Migrations
{
    /// <inheritdoc />
    public partial class Addedanindextothebookingentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Bookings_SourceId",
                table: "Bookings",
                newName: "IDX_Booking_SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_DestinationId",
                table: "Bookings",
                newName: "IDX_Booking_DestinationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IDX_Booking_SourceId",
                table: "Bookings",
                newName: "IX_Bookings_SourceId");

            migrationBuilder.RenameIndex(
                name: "IDX_Booking_DestinationId",
                table: "Bookings",
                newName: "IX_Bookings_DestinationId");
        }
    }
}
