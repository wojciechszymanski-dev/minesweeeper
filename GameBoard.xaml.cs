using LeDosSaperos.resources.Class;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LeDosSaperos
{
    public partial class GameBoard : Window
    {
        Dictionary<int, ButtonClass>? buttonDictionary;
        readonly Random rand = new();
        bool isFirstClick = true;
        readonly MainWindow mainWindow;
        int randomIdx;
        readonly int bombs = 0;
        readonly int[,] board; 
        int remainingFlags;
        readonly bool showBombs;
        DispatcherTimer timer;
        int secondsElapsed;
        readonly string difficulty;

        public GameBoard(MainWindow mainWindow, int bombs, int[,] board, bool showBombs, string difficulty)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.board = board;
            this.bombs = bombs;
            this.remainingFlags = bombs;
            this.showBombs = showBombs;
            Closing += GameBoardClosing;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();
            this.difficulty = difficulty;
        }

        public void InitializeButtons()
        {
            timerBox.Visibility = Visibility.Visible;
            buttonDictionary = new Dictionary<int, ButtonClass>();
            int gridSizeX = board.GetLength(0);
            int gridSizeY = board.GetLength(1);

            // Clear existing buttons from the grid
            innerGrid.Children.Clear();
            innerGrid.RowDefinitions.Clear(); 
            innerGrid.ColumnDefinitions.Clear(); 

            // Calculate the size of each button
            double buttonSize = Math.Min(innerGrid.ActualWidth / gridSizeY, innerGrid.ActualHeight / gridSizeX);
            double marginValue = buttonSize * 0;

            for (int i = 0; i < gridSizeX; i++)
            {
                RowDefinition rowDef = new()
                {
                    Height = new GridLength(buttonSize)
                };
                innerGrid.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < gridSizeY; i++)
            {
                ColumnDefinition colDef = new()
                {
                    Width = new GridLength(buttonSize)
                };
                innerGrid.ColumnDefinitions.Add(colDef);
            }

            // Create button grid
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    int index = i * gridSizeY + j;

                    ButtonClass myButton = new(index, false, false, false);
                    buttonDictionary[index] = myButton;

                    Button button = new()
                    {
                        Width = buttonSize,
                        Height = buttonSize,
                        Margin = new Thickness(marginValue), 
                        Padding = new Thickness(0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    button.Click += BoardButtonClicked;
                    button.MouseRightButtonUp += FlagButtonClicked; 

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    innerGrid.Children.Add(button);
                }
            }

            if (board.Length > (20*20))
            {
                timerBox.HorizontalAlignment = HorizontalAlignment.Center;
                timerBox.Margin = new Thickness(0, 0, 0, -400);
            }

            // Display the remaining flags count
            remainingFlagsTextBlock.Content = $"🚩: {remainingFlags}";
        }

        private void GenerateBombs(int exclude)
        {
            int bombsPlaced = 0;

            // Generating bombs in random indexes in button dictionary
            while (bombsPlaced < bombs)
            {
                randomIdx = rand.Next(0, board.Length);
                if (randomIdx != exclude && !buttonDictionary[randomIdx].IsBomb)
                {
                    buttonDictionary[randomIdx].IsBomb = true;
                    bombsPlaced++;
                }
            }
        }

        private void BoardButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && innerGrid.Children.Contains(button))
            {
                if (isFirstClick)
                {
                    int clickedButtonIndex = innerGrid.Children.IndexOf(button);
                    GenerateBombs(clickedButtonIndex);
                    if (showBombs)
                    {
                        foreach (var kvp in buttonDictionary)
                        {
                            if (kvp.Value.IsBomb)
                            {
                                Button bombButton = (Button)innerGrid.Children[kvp.Key];
                                bombButton.Content = "💣";
                            }
                        }
                    }
                }
                isFirstClick = false;

                // Check if the clicked button is already revealed with a number or has a flag
                if (buttonDictionary[innerGrid.Children.IndexOf(button)].IsRevealed ||
                    (button.Content != null && button.Content.ToString() == "🚩"))
                {
                    return; // Do nothing if the cell is already revealed with a number or contains a flag
                }
                ReveilCells(button);
            }
        }

        private void ReveilCells(Button button)
        {
            int clickedButtonIndex = innerGrid.Children.IndexOf(button);
            int gridSizeY = board.GetLength(1);

            // Calculate row and column indices from the clicked button index
            int rowIndex = clickedButtonIndex / gridSizeY;
            int colIndex = clickedButtonIndex % gridSizeY;

            if (buttonDictionary[clickedButtonIndex].IsBomb)
            {
                foreach (var kvp in buttonDictionary)
                {
                    if (kvp.Value.IsBomb)
                    {
                        Button bombButton = (Button)innerGrid.Children[kvp.Key];
                        bombButton.Content = "💣";
                    }
                }
                string result = "lose";
                EndGameResult(result);
                return;
            }

            // Check the surrounding cells
            for (int i = Math.Max(0, rowIndex - 1); i <= Math.Min(rowIndex + 1, board.GetLength(0) - 1); i++)
            {
                for (int j = Math.Max(0, colIndex - 1); j <= Math.Min(colIndex + 1, board.GetLength(1) - 1); j++)
                {
                    int index = i * gridSizeY + j;
                    Button nearButton = (Button)innerGrid.Children[index];
                    if (!buttonDictionary[index].IsRevealed && !buttonDictionary[index].IsFlagged)
                    {
                        if (!buttonDictionary[index].IsBomb)
                        {
                            // If the near cell is next to a bomb, display the number of adjacent bombs
                            int nearBombsCount = CountNearBombs(index);
                            if (nearBombsCount > 0)
                            {
                                nearButton.Content = nearBombsCount.ToString();
                                buttonDictionary[index].IsRevealed = true;
                                nearButton.Background = new SolidColorBrush(Colors.Gray);
                            }
                            else
                            {
                                // If there are no bombs around, recursively reveal the adjacent cells
                                nearButton.Background = new SolidColorBrush(Colors.Gray);
                                buttonDictionary[index].IsRevealed = true;

                                ReveilCells(nearButton);
                            }
                        }
                    }
                }
            }
        }

        // Calculate and return the wrapped index
        private int GetWrappedIndex(int x, int y, int gridSizeX, int gridSizeY)
        {
            if (x < 0) x = gridSizeX - 1;
            else if (x >= gridSizeX) x = 0;

            if (y < 0) y = gridSizeY - 1;
            else if (y >= gridSizeY) y = 0;

            return x * gridSizeY + y;
        }

        private int CountNearBombs(int index)
        {
            int gridSizeY = board.GetLength(1);
            int rowIndex = index / gridSizeY;
            int colIndex = index % gridSizeY;
            int nearBombsCount = 0;

            // Check the surrounding cells
            for (int i = rowIndex - 1; i <= rowIndex + 1; i++)
            {
                for (int j = colIndex - 1; j <= colIndex + 1; j++)
                {
                    if (i == rowIndex && j == colIndex) continue; 

                    int wrappedIndex = GetWrappedIndex(i, j, board.GetLength(0), gridSizeY);
                    if (buttonDictionary[wrappedIndex].IsBomb) nearBombsCount++;
                }
            }
            return nearBombsCount;
        }


        private async void FlagButtonClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && innerGrid.Children.Contains(button))
            {
                int clickedButtonIndex = innerGrid.Children.IndexOf(button);
                ButtonClass clickedButton = buttonDictionary[clickedButtonIndex];

                if (clickedButton.IsRevealed) return;
                clickedButton.IsFlagged = !clickedButton.IsFlagged;

                // Update flag icon and counts
                if (clickedButton.IsFlagged)
                {
                    if (remainingFlags > 0)
                    {
                        // Place flag
                        remainingFlags--; 
                        button.Content = "🚩"; 

                        if(remainingFlags == 0)
                        {
                            int count = 0;
                            foreach (var kvp in buttonDictionary)
                            {
                                if (kvp.Value.IsBomb && kvp.Value.IsFlagged) count++;
                            }
                            if (count == bombs)
                            {
                                string result = "win";
                                EndGameResult(result);
                            }
                        }
                    }
                }
                else
                {
                    if(button.Content != null && button.Content.ToString() == "🚩")
                    {
                        // Remove flag
                        remainingFlags++;
                        if(buttonDictionary[clickedButtonIndex].IsBomb) button.Content = ((showBombs) ? "💣" : "");
                        else button.Content = "";
                    }
                }
                buttonDictionary[clickedButtonIndex].IsFlagged = clickedButton.IsFlagged;
                remainingFlagsTextBlock.Content = $"Flags: {remainingFlags}";
            }
        }

        private async void EndGameResult(string result)
        {
            EndGame endGameWindow = new(mainWindow, this, result);
            if (result == "win")
            {
                int minutes = secondsElapsed / 60;
                int seconds = secondsElapsed % 60;
                string timerValue = $"{minutes:00}:{seconds:00}";

                SaveWinDateTime(difficulty, timerValue);
            }
            await Task.Delay(1000);
            this.Hide();
            endGameWindow.Show();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            secondsElapsed++;
            TimeSpan time = TimeSpan.FromSeconds(secondsElapsed);
            string timerDisplay = time.ToString(@"mm\:ss");
            timerBox.Content = timerDisplay;
        }

        private void SaveWinDateTime(string difficulty, string timerValue)
        {
            try
            {
                string winDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                /* string directoryPath = Path.Combine(Environment.CurrentDirectory, "resources");
                   Directory.CreateDirectory(directoryPath); 
                   string filePath = Path.Combine(directoryPath, "wins.txt");*/

                // PATHTOCHANGE
                string filePath = Path.Combine("C:\\Users\\Wojtek\\source\\repos\\LeDosSaperos\\resources\\wins.txt");

                // Write win date, time, difficulty, and timer value to the file
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{timerValue} | {winDateTime} | {difficulty} | {(showBombs ? "Bombs Shown" : "Bombs Hidden")}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while saving win date and time: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GameBoardClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Show(); 
        }
    }
}