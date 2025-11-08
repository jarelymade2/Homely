using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class EnsureAdminUserExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration ensures the admin user exists
            // The actual user creation is handled by the Seed.cs class
            // This migration just serves as a marker in the migration history
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}

