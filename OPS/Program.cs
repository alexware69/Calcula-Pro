using ElectronNET.API;
using ElectronNET.API.Entities;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseElectron(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1000);//You can set Time   
            }); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.UseSession();

if (HybridSupport.IsElectronActive)
{
    CreateElectronWindow();
}

async void CreateElectronWindow()
{
    var options = new BrowserWindowOptions
    {
        Width = 1280,
        Height = 1024,
        WebPreferences = new WebPreferences { WebSecurity = false, DevTools = false},
        AutoHideMenuBar = true,
        TitleBarStyle = TitleBarStyle.hidden
    };
    //Electron.Dock.SetIcon("../../../../wwwroot/Images/electron.png");
    var window = await Electron.WindowManager.CreateWindowAsync(options);
    window.OnClosed += () => Electron.App.Quit();
}

app.Run();



