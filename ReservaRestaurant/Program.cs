using Microsoft.EntityFrameworkCore;
using ReservaRestaurant.Repository;
using ReservaRestaurant.Repository.Interfaces;
using ReservaRestaurant.Service;
using ReservaRestaurant.Service.Interfaces;

namespace ReservaRestaurant
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ReservaRestaurantContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IReservaService, ReservaService>();
            builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();


            app.MapControllers();

            app.Run();
        }
    }
}
