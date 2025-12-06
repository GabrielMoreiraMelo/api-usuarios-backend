using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using APIUsuarios.Infrastructure.Persistence;
using APIUsuarios.Application.Interfaces;
using APIUsuarios.Infrastructure.Repositories;
using APIUsuarios.Application.Services;
using APIUsuarios.Application.DTOs;
using APIUsuarios.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UsuarioCreateDtoValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/usuarios", async (IUsuarioService service, CancellationToken ct) =>
{
    var lista = await service.ListarAsync(ct);
    return Results.Ok(lista);
});

app.MapGet("/usuarios/{id:int}", async (int id, IUsuarioService service, CancellationToken ct) =>
{
    var u = await service.ObterAsync(id, ct);
    return u is null ? Results.NotFound() : Results.Ok(u);
});

app.MapPost("/usuarios", async (UsuarioCreateDto dto, IUsuarioService service, IValidator<UsuarioCreateDto> validator, CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(dto, ct);
    if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());

    try
    {
        var created = await service.CriarAsync(dto, ct);
        return Results.Created($"/usuarios/{created.Id}", created);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
});

app.MapPut("/usuarios/{id:int}", async (int id, UsuarioUpdateDto dto, IUsuarioService service, IValidator<UsuarioUpdateDto> validator, CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(dto, ct);
    if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());

    try
    {
        var updated = await service.AtualizarAsync(id, dto, ct);
        return Results.Ok(updated);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
});

app.MapDelete("/usuarios/{id:int}", async (int id, IUsuarioService service, CancellationToken ct) =>
{
    var ok = await service.RemoverAsync(id, ct);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.Run();
