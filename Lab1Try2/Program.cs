// ��������� ������ ��� ����������
using Dapper;
using FluentValidation;
using Lab1Try2.BBL.Services;
using Lab1Try2.DAL.Interfaces;
using Lab1Try2.DAL.Repositories;
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

// �����������, ������� ������������� ������������ ��� ����������� � �������
builder.Services.AddControllers();
// ��������� swagger
builder.Services.AddSwaggerGen();

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
