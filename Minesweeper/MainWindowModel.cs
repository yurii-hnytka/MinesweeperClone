using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Data.SQLite;
using System.IO;

namespace CourseProject {
    class MainWindowModel : INotifyPropertyChanged {
        static int[][] directions = new int[][] {
                new int[] { 1, -1 },
                new int[] { 1, 0 },
                new int[] { 1, 1 },
                new int[] { 0, 1 },
                new int[] { -1, 1 },
                new int[] { -1, 0 },
                new int[] { -1, -1 },
                new int[] { 0, -1 },
            };

        int[,] bombs,
               map;

        int currentTime,
            width,
            height,
            fieldsRemaining,
            scoreboardDifficultyIndex;

        bool isGameRunning = false,
             isWon = false;

        string playerName;

        RelayCommand clearScoreboardCommand;

        DispatcherTimer timer = new DispatcherTimer();

        public int[,] Bombs {
            get {
                return this.bombs;
            }
        }

        public int CurrentTime {
            get {
                return this.currentTime;
            }
            set {
                this.currentTime = value;
                this.OnPropertyChanged("CurrentTime");
            }
        }

        public int Width {
            get {
                return this.width;

            }
        }

        public int Height {
            get {
                return this.height;
            }
        }

        public int FieldsRemaining {
            get {
                return this.fieldsRemaining;
            }

            set {
                this.fieldsRemaining = value;

                if (this.fieldsRemaining == 0) {
                    this.stopGame();
                    this.isWon = true;
                }
            }
        }

        public int ScoreboardDifficultyIndex {
            get {
                return scoreboardDifficultyIndex;

            }

            set {
                scoreboardDifficultyIndex = value;
                OnPropertyChanged("ScoreboardDifficultyIndex");
                OnPropertyChanged("Scoreboard");
            }
        }

        public bool IsWon {
            get {
                return this.isWon;
            }
        }

        public string PlayerName {
            get {
                return playerName;
            }

            set {
                playerName = value;
                OnPropertyChanged("PlayerName");
            }
        }

        public RelayCommand ClearScoreboardCommand {
            get {
                return clearScoreboardCommand ?? (clearScoreboardCommand = new RelayCommand(
                        (obj) => {
                            string tableName = ((ScoreboardDifficultyIndex == 0) ? "beginner" : (ScoreboardDifficultyIndex == 1) ? "intermediate" : "expert");

                            if (File.Exists("scoreboard.sqlite")) {
                                using (SQLiteConnection scoreboardConnection = new SQLiteConnection("Data Source=scoreboard.sqlite;")) {
                                    scoreboardConnection.Open();

                                    using (SQLiteCommand command = scoreboardConnection.CreateCommand()) {
                                        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

                                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                                            if (reader.HasRows) {
                                                reader.Close();
                                                command.CommandText = $"DELETE FROM {tableName};";
                                                command.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                    OnPropertyChanged("Scoreboard");
                                }
                            }
                        }
                    )
                );
            }
        }

        public List<string[]> Scoreboard {
            get {
                List<string[]> scoreboard = new List<string[]>();
                string tableName = ((ScoreboardDifficultyIndex == 0) ? "beginner" : (ScoreboardDifficultyIndex == 1) ? "intermediate" : "expert");

                if (File.Exists("scoreboard.sqlite")) {
                    using (SQLiteConnection scoreboardConnection = new SQLiteConnection("Data Source=scoreboard.sqlite;")) {
                        scoreboardConnection.Open();

                        using (SQLiteCommand command = scoreboardConnection.CreateCommand()) {
                            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

                            using (SQLiteDataReader reader = command.ExecuteReader()) {
                                if (reader.HasRows) {
                                    reader.Close();

                                    command.CommandText = $"SELECT Player_Name, Time FROM {tableName} ORDER BY Time";

                                    using (SQLiteDataReader reader2 = command.ExecuteReader()) {
                                        while (reader2.Read()) {
                                            scoreboard.Add(new string[] {
                                                reader2.GetValue(0).ToString(),
                                                reader2.GetValue(1).ToString(),
                                            });
                                        }

                                        reader.Close();
                                    }
                                }
                            }
                        }
                    }
                }

                return scoreboard;
            }
        }

        public MainWindowModel(int yAmount, int xAmount, int minesAmount, string PlayerName = "Player") {
            Random random = new Random();
            List<int> bombsY = new List<int>(),
                      bombsX = new List<int>();

            int[] tempCoord;
            bool isCoordsUnique;

            CurrentTime = 0;
            height = yAmount;
            width = xAmount;
            bombs = new int[yAmount, xAmount];
            map = new int[yAmount, xAmount];
            fieldsRemaining = yAmount * xAmount - minesAmount;
            this.PlayerName = PlayerName;

            Array.Clear(this.bombs, 0, this.bombs.Length);
            Array.Clear(this.map, 0, this.map.Length);

            for (int i = 0; i < minesAmount; ++i) {
                do {
                    isCoordsUnique = true;

                    tempCoord = new int[] { random.Next(0, yAmount), random.Next(0, xAmount) };

                    for (int j = 0; j < bombsX.Count; ++j) {
                        if (bombsY[j] == tempCoord[0] &&
                            bombsX[j] == tempCoord[1]) {
                            isCoordsUnique = false;
                        }
                    }
                }
                while (!isCoordsUnique);

                this.bombs[tempCoord[0], tempCoord[1]] = 1;
                this.map[tempCoord[0], tempCoord[1]] = 1;

                foreach (int[] direction in directions) {
                    int checkY = tempCoord[0] + direction[0],
                        checkX = tempCoord[1] + direction[1];

                    if (checkY >= 0 && checkY < this.map.GetLength(0)) {
                        if (checkX >= 0 && checkX < this.map.GetLength(1) && this.map[checkY, checkX] != 1) {
                            this.map[checkY, checkX] = -1;
                        }
                    }
                }

                bombsY.Add(tempCoord[0]);
                bombsX.Add(tempCoord[1]);

            }

            this.timer.Tick += new EventHandler(this.timer_Tick);
            this.timer.Interval = new TimeSpan(0, 0, 1);

        }

        public void startGame() {
            if (!this.isGameRunning) {
                this.timer.Start();
                this.isGameRunning = true;

                this.CurrentTime = 0;
            }
        }

        public void stopGame() {
            if (this.isGameRunning) {
                this.timer.Stop();
                this.isGameRunning = false;

            }
        }

        public async void saveScore(int difficultyIndex) {
            if (difficultyIndex != 3) {
                string tableName = ((difficultyIndex == 0) ? "beginner" : (difficultyIndex == 1) ? "intermediate" : "expert");

                if (!File.Exists("scoreboard.sqlite")) {
                    SQLiteConnection.CreateFile("scoreboard.sqlite");

                }

                using (SQLiteConnection scoreboardConnection = new SQLiteConnection("Data Source=scoreboard.sqlite;")) {
                    scoreboardConnection.Open();

                    using (SQLiteCommand command = scoreboardConnection.CreateCommand()) {
                        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            if (!reader.HasRows) {
                                reader.Close();

                                command.CommandText = $"CREATE TABLE '{tableName}' (" +
                                    "'ID'    INTEGER NOT NULL UNIQUE," +
                                    "'Player_Name'   TEXT NOT NULL," +
                                    "'Time'  INTEGER NOT NULL," +
                                    "PRIMARY KEY('ID' AUTOINCREMENT));";

                                command.ExecuteNonQuery();
                            }

                        }

                        command.CommandText = $"INSERT INTO {tableName} (Player_Name, Time) VALUES ('{PlayerName}', {CurrentTime})";

                        await command.ExecuteNonQueryAsync();
                    }
                }
                OnPropertyChanged("Scoreboard");
            }
        }

        public Stack<int[]> getEmptyFields(int y, int x) {
            Stack<int[]> emptyFields = new Stack<int[]>();
            int clickX = x, clickY = y;

            if (this.map[y, x] != -1) {
                bool isAreaCleared = false;
                Stack<int> clearingPath = new Stack<int>();
                int currentDirectionIndex = 0, peekX, peekY;

                emptyFields.Push(new int[] { y, x });
                this.map[y, x] = 2;

                while (isAreaCleared != true) {
                    while (currentDirectionIndex < 8) {
                        peekY = y + directions[currentDirectionIndex][0];
                        peekX = x + directions[currentDirectionIndex][1];

                        if (
                            peekY >= 0 && peekY < this.map.GetLength(0) &&
                            peekX >= 0 && peekX < this.map.GetLength(1)
                            ) {
                            if (this.map[peekY, peekX] == 0 || this.map[peekY, peekX] == -1) {
                                if (this.map[peekY, peekX] == 0) {
                                    clearingPath.Push(currentDirectionIndex);
                                    currentDirectionIndex = 0;
                                    y = peekY;
                                    x = peekX;

                                }

                                emptyFields.Push(new int[] { peekY, peekX });
                                this.map[peekY, peekX] = 2;

                            } else {
                                currentDirectionIndex++;

                            }
                        } else {
                            currentDirectionIndex++;

                        }
                    }

                    if (clearingPath.Count > 0) {
                        do {
                            y -= directions[clearingPath.Peek()][0];
                            x -= directions[clearingPath.Peek()][1];
                            currentDirectionIndex = (clearingPath.Pop() + 1) % 8;

                        } while (clearingPath.Count != 0 && currentDirectionIndex == 0);

                    } else if (x == clickX && y == clickY) {
                        isAreaCleared = true;

                    }
                }

            } else {
                emptyFields.Push(new int[] { y, x });
                this.map[y, x] = 2;

            }

            this.FieldsRemaining -= emptyFields.Count;

            return emptyFields;
        }

        public string getBombsNearby(int i, int j) {
            int bombsNearby = 0;

            foreach (int[] direction in directions) {
                int checkY = i + direction[0], checkX = j + direction[1];

                if (checkY >= 0 && checkY < this.bombs.GetLength(0)) {
                    if (checkX >= 0 && checkX < this.bombs.GetLength(1)) {
                        if (this.bombs[checkY, checkX] == 1) {
                            bombsNearby += 1;

                        }
                    }
                }
            }

            return (bombsNearby != 0) ? bombsNearby.ToString() : "";
        }

        void timer_Tick(object sender, EventArgs e) {
            if (this.CurrentTime < 1000) {
                this.CurrentTime += 1;

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
