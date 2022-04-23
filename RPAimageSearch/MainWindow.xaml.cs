using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using AForge.Imaging;
using AForge;
using AForge.Math;
using Microsoft.Win32;

namespace RPAimageSearch
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        Bitmap source;
        public MainWindow()
        {
            InitializeComponent();
        }

        //Скриншот экрана
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Thread.Sleep(200);
            scrshot.Source = ImageFromScreen();
            this.WindowState = WindowState.Maximized;
        }
        public ImageSource ImageFromScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }
            source = bmp;
            var handle = bmp.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
            }
        }
    }
    public class AforgeService
    {
        private TemplateMatch[] _matchings;
        public int CountMatchings
        {
            get => _matchings != null ? _matchings.Length : 0;
        }
        public AforgeService()
        {

        }
        public async Task<bool> IsContains(Bitmap pathOriginalImage, string pathSampleImage)
        {
            if (String.IsNullOrEmpty(pathSampleImage)) throw new ArgumentNullException(nameof(pathSampleImage));

            var sample = new Bitmap(pathSampleImage);
            var orig = pathOriginalImage;

            //пользуемся библиотекой
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.921f);
            _matchings = await Task.Run(() => tm.ProcessImage(orig, sample));

            return _matchings.Any();
        }


        public List<FoundPlace> GetPlaces()
        {
            List<FoundPlace> result = new List<FoundPlace>();
            if (CountMatchings == 0) return result;

            int id = 0;
            foreach (var match in _matchings)
            {
                FoundPlace place = new FoundPlace
                {
                    Id = ++id,
                    Similarity = match.Similarity,
                    Top = match.Rectangle.Top,
                    Left = match.Rectangle.Left,
                    Height = match.Rectangle.Height,
                    Width = match.Rectangle.Width
                };

                result.Add(place);
            }

            return result;
        }

    }
    public class FoundPlace
    {
        public int Id { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Similarity { get; set; }
    }
}
