using Microsoft.EntityFrameworkCore;
using Slotwise.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<SlotwiseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SlotwiseDb")));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
