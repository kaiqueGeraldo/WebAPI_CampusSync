using CS.Models;
using Microsoft.EntityFrameworkCore;

namespace CS.API.Data
{
    public class ProjetoDbContext : DbContext
    {
        public ProjetoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Estudante> Estudantes { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Faculdade> Faculdades { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User -> Faculdade
            modelBuilder.Entity<User>()
                .HasMany(u => u.Faculdades)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserCPF)
                .OnDelete(DeleteBehavior.Cascade); // Faculdades serão excluídas ao excluir o User.

            // Faculdade -> Cursos
            modelBuilder.Entity<Faculdade>()
                .HasMany(f => f.Cursos)
                .WithOne(c => c.Faculdade)
                .HasForeignKey(c => c.FaculdadeId)
                .OnDelete(DeleteBehavior.Cascade); // Cursos serão excluídos ao excluir a Faculdade.

            // Faculdade -> Colaboradores
            modelBuilder.Entity<User>()
                .HasMany(f => f.Colaboradores)
                .WithOne()
                .HasForeignKey(f => f.UserCPFColaboradores)
                .OnDelete(DeleteBehavior.Cascade);

            // Colaboradores -> Faculdade
            modelBuilder.Entity<Colaborador>()
                .HasOne(f => f.User)
                .WithOne()
                .HasForeignKey<Colaborador>(f => f.UserCPFColaboradores)
                .OnDelete(DeleteBehavior.NoAction);

            // Curso -> Turmas
            modelBuilder.Entity<Curso>()
                .HasMany(c => c.Turmas)
                .WithOne(t => t.Curso)
                .HasForeignKey(t => t.CursoId)
                .OnDelete(DeleteBehavior.Cascade); // Turmas serão excluídas ao excluir o Curso.

            // Curso -> Disciplinas
            modelBuilder.Entity<Curso>()
                .HasMany(c => c.Disciplinas)
                .WithOne(d => d.Curso)
                .HasForeignKey(d => d.CursoId)
                .OnDelete(DeleteBehavior.Cascade); // Disciplinas serão excluídas ao excluir o Curso.

            // Turma -> Estudantes
            modelBuilder.Entity<Turma>()
                .HasMany(t => t.Estudantes)
                .WithOne(e => e.Turma)
                .HasForeignKey(e => e.TurmaId)
                .OnDelete(DeleteBehavior.Cascade); // Estudantes serão excluídos ao excluir uma Turma.

            // Estudante -> Turma
            modelBuilder.Entity<Estudante>()
                .HasOne(e => e.Turma)
                .WithMany(t => t.Estudantes)
                .HasForeignKey(e => e.TurmaId)
                .OnDelete(DeleteBehavior.NoAction); // Turma será preservada ao excluir um Estudante.

            // Curso -> Colaborador
            modelBuilder.Entity<Curso>()
                .HasOne(c => c.Colaborador)
                .WithOne(co => co.Curso)
                .HasForeignKey<Colaborador>(co => co.CursoId)
                .OnDelete(DeleteBehavior.Cascade); // Colaborador será excluído ao excluir um Curso.

            // Colaborador -> Curso
            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Curso)
                .WithOne(co => co.Colaborador)
                .HasForeignKey<Colaborador>(co => co.CursoId)
                .OnDelete(DeleteBehavior.NoAction); // Curso será preservado ao excluir um Colaborador.

            // Estudante -> Pessoa
            modelBuilder.Entity<Estudante>()
                .HasOne(e => e.Pessoa)
                .WithOne()
                .HasForeignKey<Estudante>(e => e.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pessoa -> Endereço
            modelBuilder.Entity<Pessoa>()
                .HasOne(p => p.Endereco)
                .WithOne()
                .HasForeignKey<Pessoa>(p => p.EnderecoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Colaborador -> Pessoa
            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Pessoa)
                .WithOne()
                .HasForeignKey<Colaborador>(c => c.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
