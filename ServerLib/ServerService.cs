using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.ServiceModel;
using System.Threading;

namespace ServerLib
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServerService : IServerService
    {
        FilterManager manager = new FilterManager();

        public string[] GetFilters()
        {
            var filters = new List<string>();
            foreach (var filterName in ConfigurationManager.AppSettings)
            {
                filters.Add(filterName.ToString());
            }
            return filters.ToArray();
        }

        public void StartFilter(byte[] image, string filterName)
        {
            manager = new FilterManager();
            Monitor.Enter(manager);
            manager.isWorking = true;
            Monitor.Exit(manager);
            Bitmap inputImage;
            using (MemoryStream stream = new MemoryStream(image))
            {
                inputImage = (Bitmap)Image.FromStream(stream);
            }
            Bitmap outputImage = manager.ApplyingFilterToBitmap(inputImage, filterName);
            if (outputImage == null)
            {
                OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().StopWorking();
            }
            else
            {
                byte[] message;
                using (MemoryStream stream = new MemoryStream())
                {
                    outputImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    message = stream.GetBuffer();
                }
                OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetImage(message);
                Monitor.Enter(manager);
                manager.isWorking = false;
                Monitor.Exit(manager);
            }
        }

        public void StopFilter()
        {
            Monitor.Enter(manager);
            manager.isWorking = false;
            Monitor.Exit(manager);
        }
    }
}
