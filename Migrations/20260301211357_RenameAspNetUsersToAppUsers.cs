using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureWebAppDemo.Migrations
{
    public partial class RenameAspNetUsersToAppUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabloyu yeniden adlandır
            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AppUsers");

            // Eğer index veya primary key isimleri AspNetUsers'e göre oluşturulduysa, onları da yeniden adlandır
            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "AppUsers",
                newName: "IX_AppUsers_UserName");

            migrationBuilder.RenameIndex(
                name: "EmailIndex",
                table: "AppUsers",
                newName: "IX_AppUsers_Email");

            // Gerekirse diğer Identity tablolarını da aynı şekilde yeniden adlandır
            migrationBuilder.RenameTable(name: "AspNetRoles", newName: "Roles");
            migrationBuilder.RenameTable(name: "AspNetUserRoles", newName: "UserRoles");
            migrationBuilder.RenameTable(name: "AspNetUserClaims", newName: "UserClaims");
            migrationBuilder.RenameTable(name: "AspNetUserLogins", newName: "UserLogins");
            migrationBuilder.RenameTable(name: "AspNetRoleClaims", newName: "RoleClaims");
            migrationBuilder.RenameTable(name: "AspNetUserTokens", newName: "UserTokens");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma işlemleri
            migrationBuilder.RenameTable(name: "AppUsers", newName: "AspNetUsers");
            migrationBuilder.RenameIndex(name: "IX_AppUsers_UserName", table: "AspNetUsers", newName: "UserNameIndex");
            migrationBuilder.RenameIndex(name: "IX_AppUsers_Email", table: "AspNetUsers", newName: "EmailIndex");

            migrationBuilder.RenameTable(name: "Roles", newName: "AspNetRoles");
            migrationBuilder.RenameTable(name: "UserRoles", newName: "AspNetUserRoles");
            migrationBuilder.RenameTable(name: "UserClaims", newName: "AspNetUserClaims");
            migrationBuilder.RenameTable(name: "UserLogins", newName: "AspNetUserLogins");
            migrationBuilder.RenameTable(name: "RoleClaims", newName: "AspNetRoleClaims");
            migrationBuilder.RenameTable(name: "UserTokens", newName: "AspNetUserTokens");
        }
    }
} 
