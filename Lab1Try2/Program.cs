// ��������� ������ ��� ����������
using Dapper;
using FluentValidation;
using Lab1Try2.BBL.Services;
using Lab1Try2.Clients;
using Lab1Try2.Config;
using Lab1Try2.DAL.Interfaces;
using Lab1Try2.DAL.Repositories;
using Lab1Try2.Jobs;
using Lab1Try2.Services;
using Lab1Try2.Validators;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.NpgSql;

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddScoped<UnitOfWork>();

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
builder.Services.AddScoped<IValidatorFactory, ValidatorFactory>();

builder.Services.AddScoped<IAuditLogOrderRepository, AuditLogOrderRepository>();
builder.Services.AddScoped<AuditLogOrderService>();
builder.Services.AddScoped<V1AuditLogOrderRequestValidator>();

// �����������, ������� ������������� ������������ ��� ����������� � �������
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
//  swagger
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMqSettings"));

builder.Services.AddScoped<RabbitMqService>();


//builder.Services.AddControllers().AddJsonOptions(options =>
//{
//   options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
//});

builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetSection("DbSettings")["ConnectionString"],
        name: "PostgreSQL Health Check",
        tags: new[] { "db", "postgresql" },
        healthQuery: "SELECT COUNT(*) FROM audit_log_order;");
builder.Services.AddHostedService<OrderGenerator>();

// �������� ������ � ����������
var app = builder.Build();

// ��������� 2 ��������� ��� ��������� �������� � �������
app.UseSwagger();
app.UseSwaggerUI();

// ��������� ��������� ��� �������� � ������ ����������
app.MapControllers();
app.MapHealthChecks("/health");

//  ***     Migrations
//          
Migrations.Program.Main([]);

//  
app.Run();
