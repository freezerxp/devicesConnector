
using devicesConnector;
using devicesConnector.Common;
using devicesConnector.Helpers;



var builder = WebApplication.CreateBuilder(args);

//razor
builder.Services.AddRazorPages();

var app = builder.Build();

//--razor
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
//--razor

LogHelper.Write("Start");


new MainMapCreator().CrateMap(app);

CommandsQueueRepository.Init();

app.Run();


