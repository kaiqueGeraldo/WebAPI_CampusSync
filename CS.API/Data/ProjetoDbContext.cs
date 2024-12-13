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
            // Configuração do relacionamento User -> Faculdade
            modelBuilder.Entity<User>()
                .HasMany(u => u.Faculdades)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserCPF)
                .OnDelete(DeleteBehavior.Cascade); // Quando User for excluído, exclui as Faculdades relacionadas

            // Configuração do relacionamento Faculdade -> Curso
            modelBuilder.Entity<Faculdade>()
                .HasMany(f => f.Cursos)
                .WithOne(c => c.Faculdade)
                .HasForeignKey(c => c.FaculdadeId)
                .OnDelete(DeleteBehavior.Cascade); // Quando Faculdade for excluída, exclui os Cursos relacionados

            // Configuração do relacionamento Curso -> Turma
            modelBuilder.Entity<Curso>()
                .HasMany(c => c.Turmas)
                .WithOne(t => t.Curso)
                .HasForeignKey(t => t.CursoId)
                .OnDelete(DeleteBehavior.Cascade); // Quando Curso for excluído, exclui as Turmas

            // Configuração do relacionamento Curso -> Disciplina
            modelBuilder.Entity<Curso>()
                .HasMany(c => c.Disciplinas)
                .WithOne(d => d.Curso)
                .HasForeignKey(d => d.CursoId)
                .OnDelete(DeleteBehavior.Cascade); // Quando Curso for excluído, exclui as Disciplinas

            // Configuração do relacionamento Estudante -> Turma
            modelBuilder.Entity<Estudante>()
                .HasOne(e => e.Turma)
                .WithMany(t => t.Estudantes)
                .HasForeignKey(e => e.TurmaId)
                .OnDelete(DeleteBehavior.NoAction); // Quando Estudante for excluído, desvincula da Turma

            // Configuração do relacionamento Colaborador -> Curso
            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Curso)
                .WithOne(cr => cr.Colaborador)
                .HasForeignKey<Colaborador>(c => c.CursoId)
                .OnDelete(DeleteBehavior.SetNull);


            // Configuração do relacionamento Faculdade -> Endereco
            modelBuilder.Entity<Faculdade>()
                .HasOne(f => f.Endereco)
                .WithOne()
                .HasForeignKey<Faculdade>(f => f.Id)
                .OnDelete(DeleteBehavior.Cascade); // Quando Faculdade for excluída, exclui o Endereço

            // Configuração do relacionamento Estudante -> Endereco
            modelBuilder.Entity<Estudante>()
                .HasOne(p => p.Endereco)
                .WithOne() 
                .HasForeignKey<Estudante>(p => p.CPF) 
                .OnDelete(DeleteBehavior.Cascade); // Quando Estudante for excluído, exclui o Endereço

            // Configuração do relacionamento Colaborador -> Endereco
            modelBuilder.Entity<Colaborador>()
                .HasOne(p => p.Endereco)
                .WithOne() 
                .HasForeignKey<Colaborador>(p => p.CPF) 
                .OnDelete(DeleteBehavior.Cascade); // Quando Colaborador for excluído, exclui o Endereço
            
            // Configuração do relacionamento Pessoa -> Endereco
            modelBuilder.Entity<Pessoa>()
                .HasOne(p => p.Endereco)
                .WithOne() 
                .HasForeignKey<Pessoa>(p => p.Id) 
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }
    }
}
