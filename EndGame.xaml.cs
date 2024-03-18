using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LeDosSaperos
{
    public partial class EndGame : Window
    {
        MainWindow mainWindow;
        public EndGame(MainWindow mainWindow, GameBoard gameBoard, string result)
        {
            InitializeComponent();
            SetBackground(result);
            this.mainWindow = mainWindow;
            Closing += EndGameClosing;
        }

        private void SetBackground(string result)
        {
            // PATHTOCHANGE
            Uri imageUri = new("C:\\Users\\Wojtek\\source\\repos\\LeDosSaperos\\resources\\win-screen-saper.png");
            Uri imageUri2 = new("C:\\Users\\Wojtek\\source\\repos\\LeDosSaperos\\resources\\lose-screen-saper.png");

            BitmapImage bitmapImage = new((result == "win") ? imageUri : imageUri2);
            ImageBrush imageBrush = new(bitmapImage);
            endGameGrid.Background = imageBrush;
        }

        private void EndGameClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Show();
        }
    }
}