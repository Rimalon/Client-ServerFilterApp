using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSystem.ServerService;

namespace TestSystem
{
    class TestClass : ServerService.IServerServiceCallback
    {
        private ServerServiceClient client;

        private string[] filters;
        private Bitmap testImage;

        public int ImageSize { get { return testImage.Width * testImage.Height; } }
        

        public TestClass(string pathToImage)
        {
            client = new ServerServiceClient(new System.ServiceModel.InstanceContext(this));
            try
            {
                filters = client.GetFilters();
            }
            catch (Exception)
            {
                throw new Exception("Server is not avaliable");
            }
            try
            {
                testImage = new Bitmap(pathToImage);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
        }

        public TimeSpan Work()
        {
            try
            {
                DateTime startFilterTime = DateTime.Now;
                byte[] message;
                client = new ServerServiceClient(new System.ServiceModel.InstanceContext(this));
                using (MemoryStream stream = new MemoryStream())
                {
                    testImage.Save(stream, ImageFormat.Bmp);
                    message = stream.GetBuffer();
                }
                client.StartFilter(message, filters.First());
                return DateTime.Now - startFilterTime;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void GetImage(byte[] image)
        {
        }

        public void GetProgress(int percent)
        {
        }

        public void StopWorking()
        {
        }
    }
}
