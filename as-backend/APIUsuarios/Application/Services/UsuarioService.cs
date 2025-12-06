using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIUsuarios.Application.DTOs;
using APIUsuarios.Application.Interfaces;
using APIUsuarios.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace APIUsuarios.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        private static string HashPassword(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToHexString(bytes);
        }

        public async Task<UsuarioReadDto> CriarAsync(UsuarioCreateDto dto, CancellationToken ct)
        {
            var emailLower = dto.Email.Trim().ToLowerInvariant();

            if (await _repo.EmailExistsAsync(emailLower, ct))
                throw new InvalidOperationException("Email já cadastrado");

            if ((DateTime.UtcNow - dto.DataNascimento).TotalDays / 365.25 < 18)
                throw new InvalidOperationException("Usuário deve ter ao menos 18 anos");

            var usuario = new Usuario
            {
                Nome = dto.Nome.Trim(),
                Email = emailLower,
                Senha = HashPassword(dto.Senha),
                DataNascimento = dto.DataNascimento,
                Telefone = dto.Telefone,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            await _repo.AddAsync(usuario, ct);
            await _repo.SaveChangesAsync(ct);

            return new UsuarioReadDto(usuario.Id, usuario.Nome, usuario.Email, usuario.DataNascimento, usuario.Telefone, usuario.Ativo, usuario.DataCriacao);
        }

        public async Task<UsuarioReadDto?> ObterAsync(int id, CancellationToken ct)
        {
            var u = await _repo.GetByIdAsync(id, ct);
            if (u == null) return null;
            return new UsuarioReadDto(u.Id, u.Nome, u.Email, u.DataNascimento, u.Telefone, u.Ativo, u.DataCriacao);
        }

        public async Task<IEnumerable<UsuarioReadDto>> ListarAsync(CancellationToken ct)
        {
            var usuarios = await _repo.GetAllAsync(ct);
            return usuarios.Select(u => new UsuarioReadDto(u.Id, u.Nome, u.Email, u.DataNascimento, u.Telefone, u.Ativo, u.DataCriacao));
        }

        public async Task<UsuarioReadDto> AtualizarAsync(int id, UsuarioUpdateDto dto, CancellationToken ct)
        {
            var u = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado");

            var emailLower = dto.Email.Trim().ToLowerInvariant();
            if (u.Email != emailLower && await _repo.EmailExistsAsync(emailLower, ct))
                throw new InvalidOperationException("Email já cadastrado");

            if ((DateTime.UtcNow - dto.DataNascimento).TotalDays / 365.25 < 18)
                throw new InvalidOperationException("Usuário deve ter ao menos 18 anos");

            u.Nome = dto.Nome.Trim();
            u.Email = emailLower;
            u.DataNascimento = dto.DataNascimento;
            u.Telefone = dto.Telefone;
            u.Ativo = dto.Ativo;
            u.DataAtualizacao = DateTime.UtcNow;

            await _repo.UpdateAsync(u, ct);
            await _repo.SaveChangesAsync(ct);

            return new UsuarioReadDto(u.Id, u.Nome, u.Email, u.DataNascimento, u.Telefone, u.Ativo, u.DataCriacao);
        }

        public async Task<bool> RemoverAsync(int id, CancellationToken ct)
        {
            var u = await _repo.GetByIdAsync(id, ct);
            if (u == null) return false;

            u.Ativo = false;
            u.DataAtualizacao = DateTime.UtcNow;
            await _repo.RemoveAsync(u, ct);
            await _repo.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> EmailJaCadastradoAsync(string email, CancellationToken ct)
        {
            return await _repo.EmailExistsAsync(email.Trim().ToLowerInvariant(), ct);
        }
    }
}
