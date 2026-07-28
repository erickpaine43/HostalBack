using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VistaAzul.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AmasDeLlaves",
                columns: new[] { "Id", "CI", "NombreApellidos", "NumeroTelefono" },
                values: new object[,]
                {
                    { 1, "85031445678", "Elena Garcia Fernandez", "+5353334445" },
                    { 2, "89110298765", "Rosa Martinez Perez", "+5355556667" }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CI", "EsVIP", "NombreApellidos", "NumeroTelefono" },
                values: new object[,]
                {
                    { 1, "99010212345", false, "Juan Perez Gomez", "+5352345678" },
                    { 2, "95051254321", true, "Maria Carmen Rodriguez", "+5358765432" },
                    { 3, "98122598765", false, "Carlos Diaz Gutierrez", "+5351112223" }
                });

            migrationBuilder.InsertData(
                table: "Habitaciones",
                columns: new[] { "Numero", "EstaFueraDeServicio" },
                values: new object[,]
                {
                    { 11, false },
                    { 12, false },
                    { 13, false },
                    { 14, false },
                    { 15, false },
                    { 21, false },
                    { 22, false },
                    { 23, false },
                    { 24, false },
                    { 25, false },
                    { 31, false },
                    { 32, false },
                    { 33, false },
                    { 34, false },
                    { 35, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AmasDeLlaves",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AmasDeLlaves",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Habitaciones",
                keyColumn: "Numero",
                keyValue: 35);
        }
    }
}
