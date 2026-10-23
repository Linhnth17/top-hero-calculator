using SpendCalc.Services;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCoreLatest;
using WebMarkupMin.NUglify;

System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ResourceCatalog>();
builder.Services.AddSingleton<GearStore>();
builder.Services.AddSingleton<BrillianceStore>();
builder.Services.AddSingleton<TrialStore>();
builder.Services.AddSingleton<HeroStore>();

// Nén HTML trả về, kể cả CSS/JS nằm trong trang (JS được rút gọn tên biến)
builder.Services.AddWebMarkupMin(o =>
    {
        o.AllowMinificationInDevelopmentEnvironment = true;
        o.AllowCompressionInDevelopmentEnvironment = true;
    })
    .AddHtmlMinification(o =>
    {
        o.MinificationSettings.RemoveOptionalEndTags = false;
        o.CssMinifierFactory = new NUglifyCssMinifierFactory();
        o.JsMinifierFactory = new NUglifyJsMinifierFactory();
    })
    .AddHttpCompression(o =>
    {
        o.CompressorFactories = new List<ICompressorFactory>
        {
            new BuiltInBrotliCompressorFactory(), new GZipCompressorFactory()
        };
    });

var app = builder.Build();
app.UseWebMarkupMin();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
