using System;
using FluentValidation;
using APIUsuarios.Application.DTOs;

namespace APIUsuarios.Application.Validators
{
    public class UsuarioUpdateDtoValidator : AbstractValidator<UsuarioUpdateDto>
    {
        public UsuarioUpdateDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .Length(3, 100).WithMessage("Nome deve ter entre 3 e 100 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email em formato inválido");

            RuleFor(x => x.DataNascimento)
                .NotEmpty().WithMessage("Data de nascimento é obrigatória")
                .Must(BeAtLeast18).WithMessage("Usuário deve ter ao menos 18 anos");

            RuleFor(x => x.Telefone)
                .Matches(@"^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$").When(x => !string.IsNullOrEmpty(x.Telefone))
                .WithMessage("Telefone em formato inválido. Ex: (11) 91234-5678");
        }

        private bool BeAtLeast18(DateTime data)
        {
            return (DateTime.UtcNow - data).TotalDays / 365.25 >= 18;
        }
    }
}
