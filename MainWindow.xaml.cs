using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Threading.Tasks;


namespace LeDosSaperos
{
    public partial class MainWindow : Window
    {
        int count = 0;
        readonly GameBoard gameBoardWindow;
        int[,] board;
        public int bombs = 0;
        public bool showBombs;
        public string difficulty;
        bool isLeaderboardShown = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadLeaderboardContent();
        }

        public async void TurnOn(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            count++;

            DockPanel difficultyButtons = (DockPanel)FindName("difficultyButtons");
            if (difficultyButtons != null)
            {
                if (count % 2 == 0) difficultyButtons.Visibility = Visibility.Hidden;
                else difficultyButtons.Visibility = Visibility.Visible;
            }

            showBombsToggle.Visibility = (count % 2 == 1) ? Visibility.Visible : Visibility.Collapsed;
            customSettings.Visibility = (count % 2 == 1) ? Visibility.Visible : Visibility.Collapsed;
            leaderboardToggle.Visibility = (count % 2 == 1) ? Visibility.Visible : Visibility.Collapsed;

            // PATHTOCHANGE
            Uri imageUri = new("C:\\Users\\Wojtek\\source\\repos\\LeDosSaperos\\resources\\saper-menu-tablet-min.png");
            Uri imageUri2 = new("C:\\Users\\Wojtek\\source\\repos\\LeDosSaperos\\resources\\saper-menu-color-image.png\r\n");

            BitmapImage bitmapImage = new((count % 2 == 0) ? imageUri : imageUri2);
            ImageBrush imageBrush = new(bitmapImage);

            outerGrid.Background = imageBrush;
        }

        private void leaderboardToggleClicked(object sender, RoutedEventArgs e)
        {
            isLeaderboardShown = !isLeaderboardShown;
            if(isLeaderboardShown) leaderboardGrid.Visibility = Visibility.Visible;
            else leaderboardGrid.Visibility = Visibility.Collapsed;
        }

        private void LoadLeaderboardContent()
        {
            try
            {
                // PATHTOCHANGE
                string filePath = @"C:\Users\Wojtek\source\repos\LeDosSaperos\resources\wins.txt";

                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = lines[i],
                        Margin = new Thickness(0, i * 20, 0, 0) 
                    };
                    leaderboardGrid.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while loading leaderboard content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void DifficultyButtonClicked(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);

            if (sender is Button button)
            {
                difficulty = button.Content.ToString();
                switch (difficulty)
                {
                    case "Easy":
                        board = new int[8, 8];
                        bombs = 10;
                        break;

                    case "Medium":
                        board = new int[16, 16];
                        bombs = 40;
                        break;

                    case "Hard":
                        board = new int[16, 32];
                        bombs = 99;
                        break;
                    case "Custom":
                        int xDim = int.Parse(xDimmention.Text);
                        int yDim = int.Parse(yDimmention.Text);
                        board = new int[xDim, yDim];
                        bombs = (xDim * yDim) / 6;
                        break;
                }

                showBombs = (showBombsToggle.IsChecked) ?? false;

                GameBoard gameBoardWindow = new(this, bombs, board, showBombs, difficulty);
                if (gameBoardWindow != null)
                {
                    gameBoardWindow.Show();
                    gameBoardWindow.InitializeButtons();
                }
                this.Hide();
            }
        }
    }
}