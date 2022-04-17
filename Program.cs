
using devicesConnector;
using devicesConnector.Common;



var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();





new MainMapCreator().CrateMap(app);

CommandsQueueRepository.Init();

app.Run();


