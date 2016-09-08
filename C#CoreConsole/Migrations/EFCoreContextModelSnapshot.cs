using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ConsoleApplication.Data;

namespace CCoreConsole.Migrations
{
    [DbContext(typeof(EFCoreContext))]
    partial class EFCoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("ConsoleApplication.Data.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.HasKey("Id");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("ConsoleApplication.Data.Vote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CampusId");

                    b.Property<string>("Exam");

                    b.Property<int>("Mark");

                    b.Property<string>("Name");

                    b.Property<int?>("SessionId");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("ConsoleApplication.Data.Vote", b =>
                {
                    b.HasOne("ConsoleApplication.Data.Session")
                        .WithMany("ClassVotes")
                        .HasForeignKey("SessionId");
                });
        }
    }
}
