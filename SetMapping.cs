using System.Text;
using System.Text.Json;
using devicesConnector;
using devicesConnector.Drivers;
using devicesConnector.FiscalRegistrar.Commands;

public  class UrlMapCreator
{
    private WebApplication _app;

    public UrlMapCreator(WebApplication app)
    {
        _app = app;
    }

    public void CrateMaps()
    {
     
        CreateKkmMaps();
      
    }

    private void CreateKkmMaps()
    {
        _app.MapPost("/kkm/getStatus", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var jsonstring = await reader.ReadToEndAsync();


            


            var c = JsonSerializer.Deserialize<GetKkmStatusCommand>(jsonstring);

            var kkmH = new KkmHelper(c.Connection);

            var a = kkmH.GetStatus();


            return Results.Ok(new Answer(Answer.Statuses.Ok)
            {
                Message = "Command done!",
                Data = a
            });
        });


        _app.MapPost("/DoCommand", async (HttpContext context) =>
          {
              using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
              var jsonstring = await reader.ReadToEndAsync();


              var c = JsonSerializer.Deserialize<Command>(jsonstring);

              return Results.Ok(new Answer(Answer.Statuses.Ok)
              {
                  Message = "Command done!",
                  Data = c
              });
          });
    }

  
}