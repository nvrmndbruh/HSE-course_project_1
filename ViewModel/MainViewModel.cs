using Kursach.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kursach.ViewModel
{
    internal class MainViewModel : ViewModel
    {
        private LessonText Lesson;
        string statsFilePath = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources", "stats.txt");


        private bool isStarted;
        public bool IsStarted
        {
            get => isStarted;
            set
            {
                isStarted = value;
                OnPropertyChanged(nameof(IsStarted));
            }
        }


        public DispatcherTimer timer;

        double elapsedTime;
        public double ElapsedTime
        {
            get => Math.Round(elapsedTime, 1);
            set
            {
                elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }
        private void OnTimerTick(object sender, EventArgs e)
        {
            ElapsedTime += 0.1;
        }


        private int lineCount;
        public int LineCount
        {
            get => lineCount;
            set
            {
                lineCount = value;
                OnPropertyChanged(nameof(LineCount));
            }
        }

        #region Текущий цвет
        private string currentColor;
        public string CurrentColor
        {
            get => currentColor;
            set
            {
                currentColor = value;
                OnPropertyChanged(nameof(CurrentColor));
            }
        }
        #endregion


        #region Текущий текст и путь до файла
        public string CurrentText
        {
            get => Lesson.Text1;
            set
            {
                Lesson.Text1 = value;
                OnPropertyChanged(nameof(CurrentText));
            }
        }


        public string CurrentFilePath
        {
            get => Lesson.Source;
            set
            {
                Lesson.Source = value;
                OnPropertyChanged(nameof(CurrentFilePath));
            }
        }
        #endregion


        public string SecondText
        {
            get => Lesson.Text2;
            set
            {
                Lesson.Text2 = value;
                OnPropertyChanged(nameof(SecondText));
            }
        }

        public string LastText
        {
            get => Lesson.Text3;
            set
            {
                Lesson.Text3 = value;
                OnPropertyChanged(nameof(LastText));
            }
        }

        


        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = InputedText.Length;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }


        private string _InputedText;
        public string InputedText
        {
            get => _InputedText;
            set
            {
                _InputedText = value;
                OnPropertyChanged(nameof(InputedText));
                if (InputedText != null && InputedText != "")
                {
                    CurrentIndex = InputedText.Length;
                }
            }
        }

        private int doneLines;
        public int DoneLines
        {
            get => doneLines;
            set
            {
                doneLines = value;
                OnPropertyChanged(nameof(DoneLines));
            }
        }

        public void TextSwap(object parameter)
        {
            if ((LineCount - DoneLines) >= 3)
            {
                CurrentText = SecondText;
                SecondText = LastText;
                LastText = Lesson.GenerateText(CurrentFilePath, 77);
            }
            else if ((LineCount - DoneLines) == 2)
            {
                CurrentText = SecondText;
                SecondText = LastText;
                LastText = "";
            }
            else if ((LineCount - DoneLines) == 1)
            {
                CurrentText = SecondText.Trim();
                SecondText = "";
                LastText = "";
            }
            else
            {
                OnGenerateTextCommandExecuted(parameter);
            }
        }

        public ICommand OpenHelpCenterCommand { get; }

        void OnOpenHelpCenterCommandExecuted(object parameter)
        {
            Help.ShowHelp(null, "help.chm");
        }

        int errorCount = 0;

        #region Команды

        #region Генерация текста
        public ICommand GenerateTextCommand { get; }

        private bool CanGenerateTextCommandExecute(object parameter) => !IsStarted;

        private void OnGenerateTextCommandExecuted(object parameter)
        {
            InputedText = "";
            DoneLines = 0;
            errorCount = 0;
            symbolCount = 0;
            Lesson = new LessonText(CurrentFilePath, 77);
            if (LineCount >= 3)
            {
                CurrentText = Lesson.Text1;
                SecondText = Lesson.Text2;
                LastText = Lesson.Text3;
            }
            else if (LineCount == 2)
            {
                CurrentText = Lesson.Text1;
                SecondText = Lesson.Text2;
                LastText = "";
            }
            else if (LineCount == 1)
            {
                CurrentText = Lesson.Text1.Trim();
                SecondText = "";
                LastText = "";
            }
        }
        #endregion

        #region Смена языка

        string language;
        public string Language
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        public ICommand ChangeLanguageCommand { get; }

        private bool CanChangeLanguageCommandExecute(object parameter) => true;

        private void OnChangeLanguageCommandExecuted(object parameter)
        {
            Language = parameter.ToString();
            CurrentFilePath = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", $"{Language}.txt");
            OnCancelTestCommandExecuted(parameter);
            OnGenerateTextCommandExecuted(parameter);
        }
        #endregion

        #region Нажатие клавиш

        int[] lengths = new int[2]; // сохранение длины введенной строки до нажатия и после
        int symbolCount;            // количество введенных символов
        double accuracy;            // точность

        public ICommand KeyPressedCommand { get; }

        private void OnKeyPressedCommandExecuted(object parameter)
        {
            if (CanStartTestCommandExecute(parameter))
                OnStartTestCommandExecuted(parameter);

            if (lengths[0] == 0)
            {
                lengths[0] = InputedText.Length;
            }
            else
            {
                lengths[1] = lengths[0];
                lengths[0] = InputedText.Length;
            }

            if (InputedText.Length == CurrentText.Length)
            {
                if (InputedText[CurrentIndex - 1] != CurrentText[CurrentIndex - 1])
                    errorCount++;
                InputedText = "";
                DoneLines++;
                symbolCount += CurrentText.Length;

                if (DoneLines == LineCount)
                {
                    timer.Stop();
                    accuracy = (1 - (double)errorCount/(double)symbolCount) * 100;
                    string stats = $"Язык: {Language} | Введено строк: {LineCount} | Количество ошибок: {errorCount} | Время: {ElapsedTime} секунд | Скорость ввода: {Math.Round(symbolCount/(ElapsedTime/60), 2)} символов в минуту | Точность {Math.Round(accuracy)}%";
                    System.Windows.MessageBox.Show("Вы успешно завершили тест, со статистикой вы можете ознакомиться по нажатию кнопки 'статистика'",
                        "Тест успешно пройден",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    InputedText = "";
                    ElapsedTime = 0;
                    string newEntry = $"{DateTime.Now}: {stats}\n";
                    string existingContent = File.ReadAllText(statsFilePath);
                    string newContent = newEntry + existingContent;
                    File.WriteAllText(statsFilePath, newContent);
                    IsStarted = false;
                }

                TextSwap(parameter);
                lengths[0] = 0;
                lengths[1] = 0;
            }
            else
            {
                if (InputedText == CurrentText.Substring(0, InputedText.Length))
                {
                    CurrentColor = "#DBDBDB";
                }
                else
                {
                    CurrentColor = "#FF6323";
                    if (InputedText[CurrentIndex - 1] != CurrentText[CurrentIndex - 1] && lengths[0] > lengths[1])
                    {
                        errorCount++;
                    }
                }
            }
            
        }

        private bool CanKeyPressedCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Начать тест
        public ICommand StartTestCommand {  get; }

        private bool CanStartTestCommandExecute(object parameter) => !IsStarted;

        private void OnStartTestCommandExecuted(object parameter)
        {
            ElapsedTime = 0;
            timer.Start();
            DoneLines = 0;
            IsStarted = true;
        }
        #endregion

        #region Отменить тест
        public ICommand CancelTestCommand { get; }

        private bool CanCancelTestCommandExecute(object parameter) => IsStarted;

        private void OnCancelTestCommandExecuted(object parameter)
        {
            timer.Stop();
            symbolCount = 0;
            ElapsedTime = 0;
            InputedText = "";
            DoneLines = 0;
            IsStarted = false;
            OnGenerateTextCommandExecuted(parameter);
        }
        #endregion

        #region Закрыть окно

        public ICommand CloseApplicationCommand { get; }

        void OnCloseApplicationCommandExecute(object parameter)
        {
            App.Current.Shutdown();
        }

        #endregion

        #region Свернуть окно

        public ICommand MinimizeWindowCommand { get; }

        void OnMinimizeWindowCommandExecute(object parameter)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        #endregion

        #region Развернуть окно

        public ICommand MaximizeWindowCommand { get; }

        void OnMaximizeWindowCommandExecute(object parameter)
        {
            if(App.Current.MainWindow.WindowState == WindowState.Normal)
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                App.Current.MainWindow.WindowState = WindowState.Normal;
        }

        #endregion

        #region Открыть файл со статистикой

        public ICommand OpenStatsFileCommand { get; }

        void OnOpenStatsFileCommandExecuted(object parameter)
        {
            
            if (File.Exists(statsFilePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = statsFilePath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        bool CanOpenStatsFileCommandExecute(object parameter) => !isStarted;

        #endregion

        #region Очистить файл со статистикой

        public ICommand ClearStatsFileCommand { get; }

        void OnClearStatsFileCommandExecuted(object parameter)
        {
            if (File.Exists(statsFilePath))
            {
                var answer = System.Windows.MessageBox.Show("Вы уверены, что хотите стереть все данные из файла статистики? После удаления данный не возможно будет восстановить.", "Удалить статистику?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                    File.WriteAllText(statsFilePath, string.Empty);
            }
        }

        bool CanClearStatsFileCommandExecute(object parameter) => !isStarted;

        #endregion

        #endregion

        public MainViewModel()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += OnTimerTick;


            CurrentColor = "#DBDBDB";
            Language = "russian";
            IsStarted = false;
            LineCount = 3;
            DoneLines = 0;
            Lesson = new LessonText(Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", "russian.txt"), 77);

            #region Команды
            ChangeLanguageCommand = new DelegateCommand(OnChangeLanguageCommandExecuted, CanChangeLanguageCommandExecute);
            GenerateTextCommand = new DelegateCommand(OnGenerateTextCommandExecuted, CanGenerateTextCommandExecute);
            KeyPressedCommand = new DelegateCommand(OnKeyPressedCommandExecuted, CanKeyPressedCommandExecute);
            StartTestCommand = new DelegateCommand(OnStartTestCommandExecuted, CanStartTestCommandExecute);
            CancelTestCommand = new DelegateCommand(OnCancelTestCommandExecuted, CanCancelTestCommandExecute);
            CloseApplicationCommand = new DelegateCommand(OnCloseApplicationCommandExecute);
            MinimizeWindowCommand = new DelegateCommand(OnMinimizeWindowCommandExecute);
            MaximizeWindowCommand = new DelegateCommand(OnMaximizeWindowCommandExecute);
            OpenStatsFileCommand = new DelegateCommand(OnOpenStatsFileCommandExecuted, CanOpenStatsFileCommandExecute);
            ClearStatsFileCommand = new DelegateCommand(OnClearStatsFileCommandExecuted, CanClearStatsFileCommandExecute);
            OpenHelpCenterCommand = new DelegateCommand(OnOpenHelpCenterCommandExecuted);
            #endregion
        }
    }
}
