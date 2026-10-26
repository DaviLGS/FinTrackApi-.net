using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Configurations;

public class ContaConfiguration : IEntityTypeConfiguration<Conta>
{
    public void Configure(EntityTypeBuilder<Conta> builder)
    {
        builder.ToTable("contas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.SaldoInicial)
            .HasPrecision(18, 2);

        builder.HasData(
            new Conta { Id = 1, Nome = "Carteira", SaldoInicial = 0m },
            new Conta { Id = 2, Nome = "Conta Corrente", SaldoInicial = 0m });
    }
}
