using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace CourseProject {
    class MainWindowViewModel : INotifyPropertyChanged, IDataErrorInfo {
        int width = 9,
            height = 9,
            bombsAmount = 10,
            bombsAmountLabel = 10,
            difficultyIndex = 0,
            fieldWidth,
            fieldHeight;

        bool isCustomModeEnabled = false;
        MainWindowModel model;
        Grid mainGrid;
        BitmapImage smileImage;

        RelayCommand removeBtnCommand,
            bombPressedCommand,
            flagPlaceCommand,
            gameResetCommand,
            initializeGameCommand;

        public int FieldWidth {
            get {
                return fieldWidth;

            }

            set {
                fieldWidth = value;
                OnPropertyChanged("FieldWidth");
            }
        }

        public int FieldHeight {
            get {
                return fieldHeight;

            }

            set {
                fieldHeight = value;
                OnPropertyChanged("FieldHeight");
            }
        }

        public int Height {
            get {
                return height;

            }

            set {
                if (value > 0) {
                    height = value;

                } else {
                    MessageBox.Show("Height should be positive integer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        public int Width {
            get {
                return width;

            }

            set {
                if (value > 0) {
                    width = value;

                } else {
                    MessageBox.Show("Width should be positive integer", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        public int BombsAmount {
            get {
                return bombsAmount;

            }

            set {
                if (value < Width * Height && value > 0) {
                    bombsAmount = value;

                } else {
                    MessageBox.Show($"Bombs amount should be\n" +
                                     $"positive integer less than {Width * Height}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        public int BombsAmountLabel {
            get {
                return bombsAmountLabel;

            }

            set {
                bombsAmountLabel = value;
                OnPropertyChanged("BombsAmountLabel");
            }
        }

        public int DifficultyIndex {
            get {
                return difficultyIndex;

            }

            set {
                difficultyIndex = value;
                this.IsCustomModeEnabled = false;

                switch (difficultyIndex) {
                    case 0:
                        Width = 9;
                        Height = 9;
                        BombsAmount = 10;

                        break;

                    case 1:
                        Width = 16;
                        Height = 16;
                        BombsAmount = 40;

                        break;

                    case 2:
                        Width = 30;
                        Height = 16;
                        BombsAmount = 99;

                        break;

                    case 3:
                        this.IsCustomModeEnabled = true;

                        break;
                }
                OnPropertyChanged("DifficultyIndex");
            }
        }

        public bool IsCustomModeEnabled {
            get {
                return isCustomModeEnabled;

            }

            set {
                isCustomModeEnabled = value;
                OnPropertyChanged("IsCustomModeEnabled");
            }
        }

        public MainWindowModel Model {
            get {
                return this.model;
            }

            set {
                this.model = value;
                this.OnPropertyChanged("Model");
            }
        }

        public Grid MainGrid {
            get {
                return this.mainGrid;
            }

            set {
                this.mainGrid = value;
                this.OnPropertyChanged("MainGrid");
            }
        }

        public BitmapImage SmileImage {
            get {
                return this.smileImage;
            }

            set {
                this.smileImage = value;
                this.OnPropertyChanged("SmileImage");
            }
        }

        public RelayCommand RemoveBtnCommand {
            get {
                return this.removeBtnCommand ??
                    (this.removeBtnCommand = new RelayCommand(
                            obj => {
                                this.model.startGame();

                                Grid thisGrid = ((Grid) ((Button) obj).Parent).Parent as Grid;

                                foreach (int[] currentCoord in this.model.getEmptyFields(
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.RowProperty),
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.ColumnProperty))
                                ) {
                                    ((Grid) thisGrid.Children[currentCoord[0] * model.Width + currentCoord[1]]).Children.RemoveAt(1);

                                }

                                if (this.model.IsWon) {
                                    this.SmileImage = new BitmapImage(new Uri("pack://application:,,,/resources/smile2.png"));

                                    foreach (Grid cell in this.MainGrid.Children) {
                                        if (this.model.Bombs[(int) cell.GetValue(Grid.RowProperty), (int) cell.GetValue(Grid.ColumnProperty)] == 1) {
                                            ((Button) cell.Children[1]).Command = null;

                                        }
                                    }

                                    if (MessageBox.Show($"Save your score?\nScore: {Model.PlayerName} - {Model.CurrentTime}",
                                        "Save Score",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.Yes) {

                                        Model.saveScore(DifficultyIndex);

                                    }
                                }
                            }
                        )
                    );
            }
        }

        public RelayCommand BombPressedCommand {
            get {
                return this.bombPressedCommand ??
                    (this.bombPressedCommand = new RelayCommand(
                            obj => {
                                foreach (Grid cell in this.MainGrid.Children) {
                                    if (this.model.Bombs[(int) cell.GetValue(Grid.RowProperty), (int) cell.GetValue(Grid.ColumnProperty)] == 1) {
                                        cell.Children.Clear();
                                        cell.Children.Add(new Image { Source = new BitmapImage(new Uri("pack://application:,,,/resources/bomb.png")) });

                                    } else {
                                        cell.IsEnabled = false;
                                    }
                                }

                                this.model.stopGame();
                                this.SmileImage = new BitmapImage(new Uri("pack://application:,,,/resources/smile3.png"));
                            }
                        )
                    );
            }
        }

        public RelayCommand FlagPlaceCommand {
            get {
                return this.flagPlaceCommand ??
                    (this.flagPlaceCommand = new RelayCommand(
                        obj => {
                            if (((Button) obj).Background as ImageBrush == null) {
                                ((Button) ((Grid) this.MainGrid.Children[
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.RowProperty) * model.Width +
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.ColumnProperty)]
                                ).Children[1]).Background = new ImageBrush(
                                    new BitmapImage(
                                        new Uri("pack://application:,,,/resources/flag.png")
                                        )
                                    );

                            } else {
                                ((Button) ((Grid) this.MainGrid.Children[
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.RowProperty) * model.Width +
                                    (int) ((Grid) ((Button) obj).Parent).GetValue(Grid.ColumnProperty)]
                                ).Children[1]).Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));

                            }
                        }
                    )
                );
            }
        }

        public RelayCommand GameResetCommand {
            get {
                return this.gameResetCommand ?? (this.gameResetCommand = new RelayCommand(
                        obj => {
                            this.initializeGame();
                        }
                    )
                );
            }
        }

        public RelayCommand InitializeGameCommand {
            get {
                return initializeGameCommand ?? (initializeGameCommand = new RelayCommand(
                        obj => {
                            if (BombsAmount < Width * Height) {
                                BombsAmountLabel = bombsAmount;
                                initializeGame();

                            } else {
                                MessageBox.Show($"Bombs amount should be\n" +
                                    $"positive integer less than {Width * Height}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            }
                        }
                    )
                );
            }
        }

        public MainWindowViewModel() {
            this.initializeGame();
        }

        public void initializeGame() {
            Model = new MainWindowModel(height, width, bombsAmount, (Model != null) ? Model.PlayerName : "Player");
            FieldWidth = 24 * Width;
            FieldHeight = 24 * Height;

            Grid tempInnerGrid, tempOuterGrid = new Grid();
            Button tempButton;
            TextBlock tempTextBlock;
            InputBinding tempInputBinding;

            this.SmileImage = new BitmapImage(new Uri("pack://application:,,,/resources/smile.png"));

            for (int i = 0; i < model.Height; ++i) {
                tempOuterGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < model.Width; ++i) {
                tempOuterGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < model.Height; ++i) {
                for (int j = 0; j < model.Width; ++j) {
                    tempTextBlock = new TextBlock();
                    tempButton = new Button();
                    tempInnerGrid = new Grid();
                    tempInputBinding = new InputBinding(
                        this.FlagPlaceCommand,
                        new MouseGesture(MouseAction.RightClick)
                    );

                    tempTextBlock.FontSize = 24;
                    tempTextBlock.FontFamily = new FontFamily("Arial Black");
                    tempTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    tempTextBlock.VerticalAlignment = VerticalAlignment.Center;

                    tempInputBinding.CommandParameter = tempButton;
                    tempButton.InputBindings.Add(tempInputBinding);
                    tempButton.Height = 24;
                    tempButton.Width = 24;

                    if (this.model.Bombs[i, j] == 1) {
                        tempButton.Command = this.BombPressedCommand;

                    } else {
                        tempTextBlock.Text = this.model.getBombsNearby(i, j);
                        tempButton.Command = this.RemoveBtnCommand;
                        tempButton.CommandParameter = tempButton;
                    }

                    tempInnerGrid.SetValue(Grid.RowProperty, i);
                    tempInnerGrid.SetValue(Grid.ColumnProperty, j);

                    tempInnerGrid.Children.Add(tempTextBlock);
                    tempInnerGrid.Children.Add(tempButton);
                    tempOuterGrid.Children.Add(tempInnerGrid);
                }
            }

            this.MainGrid = tempOuterGrid;
        }

        public string this[string prop] {
            get {
                string error = string.Empty;

                switch (prop) {
                    case "Height":
                        if (Height <= 0) {
                            error = "Height cannot be negative";
                        }
                        break;

                    case "Width":
                        if (Width <= 0) {
                            error = "Width cannot be negative";
                        }
                        break;

                    case "BombsAmount":
                        if (BombsAmount > Height * Width - 1) {
                            error = "BombsAmount cannot be greater than fields amount alse at least one empty field required";
                        }
                        break;
                }

                return error;
            }
        }

        public string Error {
            get {
                throw new NotImplementedException();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
