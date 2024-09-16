using Azure.Storage.Blobs;
using FoodService_API.Data;
using FoodService_API.Models;
using FoodService_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register database 
builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodServiceDbConnectionString"));
});

// Register BlobServiceClient 
builder.Services.AddSingleton(u => new BlobServiceClient
(builder.Configuration.GetConnectionString("StorageAccount")));

// Register Blob service 
builder.Services.AddSingleton<IBlobService, BlobService>();

//Register Identiy
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
