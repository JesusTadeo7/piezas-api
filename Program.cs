using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔌 conexión a SQL Server
var conn = Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn, o =>
        o.EnableRetryOnFailure()

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


//  GET todos
app.MapGet("/piezas", async (AppDbContext db) =>
    await db.Piezas.ToListAsync());


//  GET por ID
app.MapGet("/piezas/{id}", async (int id, AppDbContext db) =>
{
    var pieza = await db.Piezas.FindAsync(id);
    return pieza is not null ? Results.Ok(pieza) : Results.NotFound();
});


//  POST (crear)
app.MapPost("/piezas", async (Pieza pieza, AppDbContext db) =>
{
    db.Piezas.Add(pieza);
    await db.SaveChangesAsync();
    return Results.Ok(pieza);
});


//  PUT (editar)
app.MapPut("/piezas/{id}", async (int id, Pieza input, AppDbContext db) =>
{
    var pieza = await db.Piezas.FindAsync(id);
    if (pieza is null) return Results.NotFound();

    pieza.Nombre = input.Nombre;
    pieza.Marca = input.Marca;
    pieza.Modelo = input.Modelo;
    pieza.Año = input.Año;
    pieza.Tipo = input.Tipo;

    await db.SaveChangesAsync();
    return Results.Ok(pieza);
});


//  DELETE
app.MapDelete("/piezas/{id}", async (int id, AppDbContext db) =>
{
    var pieza = await db.Piezas.FindAsync(id);
    if (pieza is null) return Results.NotFound();

    db.Piezas.Remove(pieza);
    await db.SaveChangesAsync();
    return Results.Ok();
});


app.Run();


//  modelo
class Pieza
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int Año { get; set; }
    public string? Tipo { get; set; }
}


//  contexto
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pieza> Piezas => Set<Pieza>();
}
