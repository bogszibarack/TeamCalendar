using ExciteApi.Data;
using ExciteApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// In-Memory adatbázis regisztrációja a Service Container-be
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ExciteTeamLeaveDb"));

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Az Angular címe
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger/OpenAPI 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Adatbázis létrehozása és adatok beültetése
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated(); // OnModelCreating
}

// HTTP kérés (Middleware pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // swagger a böngészőben
}

// Aktiváljuk a CORS-t
app.UseCors("AllowAngular");



// Tagok lekérése
app.MapGet("/api/team", async (AppDbContext context) =>
{
    var members = await context.TeamMembers.ToListAsync();
    return Results.Ok(members);
});

// Szabadság lekérése
app.MapGet("/api/leave", async (AppDbContext context) =>
{
    var leaves = await context.LeaveRequests
        .Include(l => l.TeamMember) // TeamMember azon. ID alapján
        .ToListAsync();
    return Results.Ok(leaves);
});

// Szabadság kérelem létrehozása valid.
app.MapPost("/api/leave", async (AppDbContext context, LeaveRequest newRequest) =>
{
    // Valid: A végdátum ne lehessen korábbi, mint a kezdődátum
    if (newRequest.StartDate > newRequest.EndDate)
    {
        return Results.BadRequest("Start date cannot be later than end date!");
    }

    // Overlap
    bool hasOverlap = await context.LeaveRequests
        .AnyAsync(l => l.TeamMemberId == newRequest.TeamMemberId &&
                       l.Status != LeaveStatus.Rejected && // A visszautasított szabadság nem számít ütközésnek
                       newRequest.StartDate <= l.EndDate && 
                       newRequest.EndDate >= l.StartDate);

    if (hasOverlap)
    {
        return Results.Conflict("This team member already has an approved or pending leave request for this period!");
    }

    // Ha nincs ütközés, elmentjük. Alapértelmezetten Pending státuszt kap
    newRequest.Status = LeaveStatus.Pending;
    context.LeaveRequests.Add(newRequest);
    await context.SaveChangesAsync();

    return Results.Created($"/api/leave/{newRequest.Id}", newRequest);
});

// Szabadság frissítése (Approved / Rejected)
app.MapPut("/api/leave/{id}/status", async (int id, LeaveStatus newStatus, AppDbContext context) =>
{
    var leave = await context.LeaveRequests.FindAsync(id);
    if (leave == null) return Results.NotFound("Leave request not found.");

    leave.Status = newStatus;
    await context.SaveChangesAsync();

    return Results.Ok(leave);
});

// On-Call lekérése vizsgálattal
app.MapGet("/api/oncall", async (AppDbContext context) =>
{
    var team = await context.TeamMembers.OrderBy(t => t.Id).ToListAsync();
    var onCallSchedule = new List<OnCallWeekDto>();

    // Kiindulási alapnak vegyük a mai napot tartalmazó hét elejét (hétfőt)
    DateTime currentDateTime = DateTime.Today;
    int daysFromMonday = ((int)currentDateTime.DayOfWeek - 1 + 7) % 7; 
// A .NET-ben a vasárnap a 0. nap. Ezzel a modulo (maradékos) osztással eltoljuk a logikát úgy, 
// hogy a Hétfő legyen a 0 (0 nap eltolás), és a Vasárnap a 6 (6 nap eltolás).
// A "+ 7" biztosítja, hogy a kivonás után se kapjunk negatív számot.
    DateTime currentMonday = currentDateTime.AddDays(-daysFromMonday);

    // Gen. a következő 12 hetet a tesztelésre
    for (int i = 0; i < 12; i++)
    {
        DateTime weekStart = currentMonday.AddDays(i * 7);
        DateTime weekEnd = weekStart.AddDays(6);

        // ISO heti sorszám a .NET beépített eszközével (A hét első napja mindig a Hétfő)
        int isoWeek = System.Globalization.ISOWeek.GetWeekOfYear(weekStart);

        // Igazságos heti rotáció kiszámítása modulo segítségével a 4 tagra
        int memberIndex = isoWeek % team.Count;
        var assignedMember = team[memberIndex];

        // Megnézzük, van-e a tagnak APPROVED szabadsága, ami beleesik ebbe a hétbe
        bool hasApprovedLeaveInWeek = await context.LeaveRequests
            .AnyAsync(l => l.TeamMemberId == assignedMember.Id &&
                           l.Status == LeaveStatus.Approved &&
                           l.StartDate <= weekEnd &&
                           l.EndDate >= weekStart);

        onCallSchedule.Add(new OnCallWeekDto
        {
            WeekNumber = isoWeek,
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            TeamMemberId = assignedMember.Id,
            TeamMemberName = assignedMember.Name,
            IsConflict = hasApprovedLeaveInWeek // Ha szabin van, UI-nak ezt ki kell emelnie!
        });
    }

    return Results.Ok(onCallSchedule);
});

app.Run();

public class OnCallWeekDto //kiszámított ügyeleti heteket továbbítja az Angular frontend felé. (adatb. modell külön a felületi megjelenítéstől)
{
    public int WeekNumber { get; set; }
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int TeamMemberId { get; set; }
    public string TeamMemberName { get; set; } = string.Empty;
    public bool IsConflict { get; set; }
}