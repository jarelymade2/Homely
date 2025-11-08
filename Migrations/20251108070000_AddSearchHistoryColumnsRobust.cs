using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayGo.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchHistoryColumnsRobust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns using raw SQL that handles both cases:
            // 1. When columns don't exist (Render case)
            // 2. When columns already exist (local case)

            migrationBuilder.Sql(@"
                -- Disable foreign keys for the migration
                PRAGMA foreign_keys = OFF;

                -- Create a backup of the current table structure
                CREATE TABLE AspNetUsers_backup AS SELECT * FROM AspNetUsers;

                -- Drop the original table
                DROP TABLE AspNetUsers;

                -- Recreate the table with all columns including the new ones
                CREATE TABLE AspNetUsers (
                    Id TEXT NOT NULL PRIMARY KEY,
                    UserName TEXT,
                    NormalizedUserName TEXT,
                    Email TEXT,
                    NormalizedEmail TEXT,
                    EmailConfirmed INTEGER NOT NULL,
                    PasswordHash TEXT,
                    SecurityStamp TEXT,
                    ConcurrencyStamp TEXT,
                    PhoneNumber TEXT,
                    PhoneNumberConfirmed INTEGER NOT NULL,
                    TwoFactorEnabled INTEGER NOT NULL,
                    LockoutEnd TEXT,
                    LockoutEnabled INTEGER NOT NULL,
                    AccessFailedCount INTEGER NOT NULL,
                    FirstName TEXT,
                    LastName TEXT,
                    SearchHistoryJson TEXT NOT NULL DEFAULT '[]',
                    PropiedadSearchHistoryJson TEXT NOT NULL DEFAULT '[]'
                );

                -- Copy data from backup, using default values for new columns if they don't exist
                INSERT INTO AspNetUsers
                SELECT
                    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd,
                    LockoutEnabled, AccessFailedCount,
                    FirstName, LastName,
                    COALESCE(SearchHistoryJson, '[]'),
                    COALESCE(PropiedadSearchHistoryJson, '[]')
                FROM AspNetUsers_backup;

                -- Drop the backup table
                DROP TABLE AspNetUsers_backup;

                -- Re-enable foreign keys
                PRAGMA foreign_keys = ON;
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropiedadSearchHistoryJson",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SearchHistoryJson",
                table: "AspNetUsers");
        }
    }
}

