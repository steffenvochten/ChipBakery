using ChipBakery.Web.Components;
using ChipBakery.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("Inventory", client => client.BaseAddress = new Uri("https://inventory-service"));
builder.Services.AddHttpClient("Order", client => client.BaseAddress = new Uri("https://order-service"));
builder.Services.AddHttpClient("Warehouse", client => client.BaseAddress = new Uri("https://warehouse-service"));
builder.Services.AddHttpClient("Loyalty", client => client.BaseAddress = new Uri("https://loyalty-service"));
builder.Services.AddHttpClient("Production", client => client.BaseAddress = new Uri("https://production-service"));
builder.Services.AddHttpClient("Supplier", c => c.BaseAddress = new Uri("https://supplier-service"));
builder.Services.AddScoped<BakeryApiClient>();
builder.Services.AddScoped<TimezoneService>();
builder.Services.AddSingleton<AgentActivityClient>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
