using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApplicationUser.Models;


var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddSingleton<GoogleDriveService>();
var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // מקסימום 50MB לקובץ
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    // Handle file uploads in Swagger
    c.OperationFilter<FileUploadOperationFilterService>();
});

builder.Services.AddDbContext<WebApplicationUserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DocumentContext")));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000") // אפשר קריאות מ- React
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();
// **הפעלת המיגרציות אוטומטית בזמן עליית השרת**
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WebApplicationUserDbContext>();
    dbContext.Database.Migrate(); 
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // ✅ מציג שגיאות מפורטות
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins); // הוסף את זה כאן

app.UseAuthorization();

app.MapControllers();

app.Run();
