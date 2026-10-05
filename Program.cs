using SpendCalc.Services;

System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<EventStore>();
builder.Services.AddSingleton<ResourceCatalog>();
builder.Services.AddSingleton<GearStore>();
builder.Services.AddSingleton<BrillianceStore>();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Gear}/{action=Index}/{id?}");
app.Run();
