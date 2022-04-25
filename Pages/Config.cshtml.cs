using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using devicesConnector.Common;
using devicesConnector.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace devicesConnector.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ConfigModel : PageModel
    {
        public IEnumerable<SelectListItem> AllDevices { get; set; }

        public IEnumerable<SelectListItem> DevicesTypes { get; set; }


        public IEnumerable<SelectListItem> KkmTypes { get; set; }

        
        public string SelectedDeviceType { get; set; }

      
        public string DeviceId { get; set; }

      
        public string SelectedKkmType { get; set; }

        public Device SelectedDevice() 
        {
            var cr = new ConfigRepository();
            var config = cr.Get();

            var d = config.Devices.SingleOrDefault(x => x.Id == DeviceId);


            if (d != null)
            {
                return d;
            }
            else
            {
                return new Device();
            }
            
        }


        private Config _config;

        private void Load()
        {
            var cr = new ConfigRepository();
            _config = cr.Get();

            AllDevices = _config.Devices.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.Name
            });


            DevicesTypes = new List<SelectListItem>
            {
                new("ККМ", ((int)Enums.DeviceTypes.FiscalRegistrar).ToString()),
                new("Весы", ((int)Enums.DeviceTypes.Scale).ToString())
            };

            KkmTypes = new List<SelectListItem>
            {
                new("АТОЛ 10", ((int)FiscalRegistrar.Objects.Enums.KkmTypes.Atol10).ToString()),
                new("ККМ Сервер", ((int)FiscalRegistrar.Objects.Enums.KkmTypes.KkmServer).ToString()),
                new("Штрих-М", ((int)FiscalRegistrar.Objects.Enums.KkmTypes.ShtrihM).ToString()),
                new("Вики Принт", ((int)FiscalRegistrar.Objects.Enums.KkmTypes.VikiPrint).ToString()),
            };

        }



        public void OnGet(string? deviceId, string? action)
        {
          
            Load();

            if (deviceId != null)
            {
                DeviceId = deviceId;
            }

            if (action == "edit" && deviceId != null)
            {
                var d = _config.Devices.SingleOrDefault(x => x.Id == deviceId);

                if (d != null)
                {
                    SelectedDeviceType = ((int)d.Type).ToString();

                    SelectedKkmType = ((int) d.SubType).ToString();
                }
            }



        }

        public IActionResult OnPost(FormDevice dev)
        {


            var cr = new ConfigRepository();

            var c = cr.Get();


            var d = c.Devices.SingleOrDefault(x => x.Id == dev.deviceId);

            if (d != null)
            {
                d.Id = dev.deviceId;
                d.Name = dev.deviceName;
            }

            cr.Save(c);

            


            return RedirectToPage("/Config");
        }

        public record class FormDevice
        {
            public string deviceId { get; set; }

            public string deviceName { get; set; }

            public  string deviceType { get; set; }
        }


        public void Do()
        {

        }
    }
}