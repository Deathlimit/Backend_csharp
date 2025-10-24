// ��������� ������ ��� ����������
using Dapper;
using FluentValidation;
using Lab1Try2.BBL.Services;
using Lab1Try2.Config;
using Lab1Try2.DAL.Interfaces;
using Lab1Try2.DAL.Repositories;
using Lab1Try2.Services;
using Lab1Try2.Validators;

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
builder.Services.AddControllers();
// ��������� swagger
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMqSettings"));

builder.Services.AddScoped<RabbitMqService>();

// �������� ������ � ����������
var app = builder.Build();

// ��������� 2 ��������� ��� ��������� �������� � �������
app.UseSwagger();
app.UseSwaggerUI();

// ��������� ��������� ��� �������� � ������ ����������
app.MapControllers();

// ������ *** ������ ���� ���� � ������� Migrations
// �� ���� � ���� ������ ����� ����������� ������� �������� �� ����
Migrations.Program.Main([]);

// �������� ����������
app.Run();
