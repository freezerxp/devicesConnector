
using devicesConnector;
using devicesConnector.Common;
using devicesConnector.Helpers;



var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();


LogHelper.Write("Start");


new MainMapCreator().CrateMap(app);

CommandsQueueRepository.Init();

app.Run();


