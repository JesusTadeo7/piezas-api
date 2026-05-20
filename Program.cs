using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

// 🔌 conexión a PostgreSQL (Render)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL")));

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


// =======================
// 🔵 CRUD PIEZAS
// =======================

// GET todos
app.MapGet("/piezas", async (AppDbContext db) =>
    await db.Piezas.ToListAsync());

// GET por ID
app.MapGet("/piezas/{id}", async (int id, AppDbContext db) =>
{
    var pieza = await db.Piezas.FindAsync(id);
    return pieza is not null ? Results.Ok(pieza) : Results.NotFound();
});

// POST
app.MapPost("/piezas", async (Pieza pieza, AppDbContext db) =>
{
    db.Piezas.Add(pieza);
    await db.SaveChangesAsync();
    return Results.Ok(pieza);
});

// PUT
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

// DELETE
app.MapDelete("/piezas/{id}", async (int id, AppDbContext db) =>
{
    var pieza = await db.Piezas.FindAsync(id);
    if (pieza is null) return Results.NotFound();

    db.Piezas.Remove(pieza);
    await db.SaveChangesAsync();
    return Results.Ok();
});


// =======================
// 👤 USERS (ALTA / BAJA)
// =======================

// GET usuarios
app.MapGet("/users", async (AppDbContext db) =>
    await db.Users.ToListAsync());

// POST usuario (CREAR)
app.MapPost("/users", async (User user, AppDbContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

// DELETE usuario
app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok();
});


// =======================
// 🔐 LOGIN (NUEVO)
// =======================
app.MapPost("/login", async (User input, AppDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u =>
            u.Username == input.Username &&
            u.Password == input.Password);

    if (user is null)
        return Results.Unauthorized();

    return Results.Ok(new
    {
        user.Id,
        user.Username,
        user.Role
    });
});


app.Run();


// =======================
// 📦 MODELOS
// =======================

class Pieza
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int Año { get; set; }
    public string? Tipo { get; set; }
}

[Table("users")]
class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = "";

    [Column("password")]
    public string Password { get; set; } = "";

    [Column("role")]
    public string Role { get; set; } = "";
}


// =======================
// 🧠 DB CONTEXT
// =======================

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pieza> Piezas => Set<Pieza>();
    public DbSet<User> Users => Set<User>();
}
