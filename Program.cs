using courses_dotnet_api.Src.Data;
using courses_dotnet_api.Src.Extensions;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.ConfigureApp();
app.Run();
