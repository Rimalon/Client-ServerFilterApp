using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using Client.ServerService;
using System.Windows.Interop;
using System.Drawing.Imaging;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ServerService.IServerServiceCallback
    {
        private Bitmap _image;
        private bool _isImageUpload;
        private volatile bool _isSendButton;
        private ServerServiceClient _client;

        public MainWindow()
        {

            InitializeComponent();
            _isImageUpload = false;
            _isSendButton = true;
            _client = new ServerServiceClient(new System.ServiceModel.InstanceContext(this));
            try
            {
                var filters = _client.GetFilters();
                foreach (var f in filters)
                {
                    filterComboBox.Items.Add(f);
                }
                filterComboBox.SelectedItem = filters.First();
            }
            catch
            {
                MessageBox.Show("Server is not avaliable");
                Close();
            }
        }
        
        public void GetImage(byte[] image)
        {
            progressBar.Value = 100;
            Bitmap bmp;
            using (MemoryStream stream = new MemoryStream(image))
            {
                bmp = (Bitmap)Image.FromStream(stream);
            }
            var imgSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero,
                     Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(_image.Width, _image.Height));
            img.Source = imgSource;
            progressBar.Value = 0;
            if (!_isSendButton)
            {
                _isSendButton = true;
                sendOrCancelBtn.Content = "Send";
            }
            _client = null;
        }

        public void GetProgress(int percent)
        {
            if (!_isSendButton)
            {
                progressBar.Value = percent;
            }
            else
            {
                progressBar.Value = 0;
            }
        }

        public void StopWorking()
        {
            _client = null;
            _isSendButton = true;
            sendOrCancelBtn.Content = "Send";
            progressBar.Value = 0;
        }

        private void selectImgBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog() { Filter = "Images (*.jpg;*.png;*.bmp) | *.jpg;*.png;*.bmp" };
            if (fileDialog.ShowDialog() == true)
            {
                selectImgBtn.Content = fileDialog.FileName;
                _image = new Bitmap(selectImgBtn.Content.ToString());
                var imgSource = Imaging.CreateBitmapSourceFromHBitmap(_image.GetHbitmap(), IntPtr.Zero,
                     Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(_image.Width, _image.Height));
                img.Source = imgSource;
                _isImageUpload = true;
            }
            else
            {
                selectImgBtn.Content = "Select Image";
                img.Source = null;
                _isImageUpload = false;
            }
        }

        private void sendOrCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isImageUpload)
            {
                if (_isSendButton)
                {
                    _isSendButton = false;
                    sendOrCancelBtn.Content = "Cancel";
                    byte[] message;
                    _client = new ServerServiceClient(new System.ServiceModel.InstanceContext(this));
                    using (MemoryStream stream = new MemoryStream())
                    {
                        _image.Save(stream, ImageFormat.Bmp);
                        message = stream.GetBuffer();
                    }
                    _client.StartFilter(message, filterComboBox.SelectedItem.ToString());
                }
                else
                {
                    _client.StopFilter();
                }
            }
            else
            {
                MessageBox.Show("Select image please");
            }
        }

    }
}
