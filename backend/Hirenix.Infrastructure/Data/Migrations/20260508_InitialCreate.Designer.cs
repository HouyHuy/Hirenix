using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hirenix.Infrastructure.Data.Migrations
{
    [DbContext(typeof(HirenixDbContext))]
    [Migration("20260508_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);
            // (Nội dung snapshot tương tự file Snapshot kia, EF dùng để link migration với snapshot)
#pragma warning restore 612, 618
        }
    }
}
