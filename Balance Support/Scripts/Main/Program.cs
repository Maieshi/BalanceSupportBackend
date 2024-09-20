
using Balance_Support.Scripts.Main.Initializers;

var builder = WebApplication.CreateBuilder(args);

await ServicesInitializer.Initialize(builder);
var app = builder.Build();
await AppInitializer.Initialize(app);
await app.RunAsync();
