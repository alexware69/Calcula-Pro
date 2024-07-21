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


BrowserWindowOptions options;
// Check if it's the first run
FirstRunManager.IsFirstRun();

options = new BrowserWindowOptions
{
    Width = FirstRunManager._data!.Width,
    Height = FirstRunManager._data.Height,
    WebPreferences = new WebPreferences { WebSecurity = false, DevTools = false},
    AutoHideMenuBar = true,
    Frame = false
};


async void CreateElectronWindow()
{
    var window = await Electron.WindowManager.CreateWindowAsync(options);
    window.OnClosed += () => Electron.App.Quit();
    //window.OnResize += () => FirstRunManager.startedMaximized = false;
}

if (HybridSupport.IsElectronActive)
{
    CreateElectronWindow();
}

app.Run();



