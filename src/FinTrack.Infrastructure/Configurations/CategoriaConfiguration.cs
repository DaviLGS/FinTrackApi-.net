using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Nome).IsUnique();

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasData(
            new Categoria { Id = 1, Nome = "Salário", Tipo = TipoTransacao.Receita },
            new Categoria { Id = 2, Nome = "Freelance", Tipo = TipoTransacao.Receita },
            new Categoria { Id = 3, Nome = "Alimentação", Tipo = TipoTransacao.Despesa },
            new Categoria { Id = 4, Nome = "Transporte", Tipo = TipoTransacao.Despesa },
            new Categoria { Id = 5, Nome = "Moradia", Tipo = TipoTransacao.Despesa },
            new Categoria { Id = 6, Nome = "Lazer", Tipo = TipoTransacao.Despesa });
    }
}
