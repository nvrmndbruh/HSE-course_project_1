using Kursach.Model;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Kursach.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        #region Lesson

        LessonText lesson;

        public LessonText Lesson
        {
            get => lesson;
            set
            {
                lesson = value;
                OnPropertyChanged(nameof(Lesson));
            }
        }

        #endregion

        #region Timer

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

        #endregion

        readonly string statsFilePath = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources", "stats.txt");

        private bool isStarted;
        public bool IsStarted
        {
            get => isStarted;
            set
            {
                isStarted = value;
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(IsStartedInverted));

            }
        }

        public bool IsStartedInverted => !isStarted;

        private int lines;
        public int Lines
        {
            get => lines;
            set
            {
                lines = value;
                OnPropertyChanged(nameof(Lines));
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

        private int currentIndex;
        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                currentIndex = TypedText.Length;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }


        private string typedText;
        public string TypedText
        {
            get => typedText;
            set
            {
                typedText = value;
                OnPropertyChanged(nameof(TypedText));

                if (TypedText == null || TypedText == "")
                    CurrentIndex = 1;
                else
                    CurrentIndex = TypedText.Length - 1;
                
            }
        }

        

        public void TextSwap(object parameter)
        {
            TypedText = "";

            if ((Lines - DoneLines) >= 3)
            {
                Lesson.CurrentText = Lesson.NextText;
                Lesson.NextText = Lesson.LastText;
                Lesson.LastText = Lesson.GenerateText(Lesson.Source, 77);
            }
            else if ((Lines - DoneLines) == 2)
            {
                Lesson.CurrentText = Lesson.NextText;
                Lesson.NextText = Lesson.LastText;
                Lesson.LastText = "";
            }
            else if ((Lines - DoneLines) == 1)
            {
                Lesson.CurrentText = Lesson.NextText.Trim();
                Lesson.NextText = "";
                Lesson.LastText = "";
            }
            else
            {
                OnGenerateTextCommandExecuted(parameter);
            }    
        }

        #region Команды

        #region Генерация текста
        public ICommand GenerateTextCommand { get; }

        private bool CanGenerateTextCommandExecute(object parameter) => !IsStarted;

        private void OnGenerateTextCommandExecuted(object parameter)
        {
            TypedText = "";
            DoneLines = 0;
            errorCount = 0;
            symbolCount = 0;

            Lesson = new LessonText(Lesson.Source, 77);

            if (Lines == 2)
            {
                Lesson.LastText = "";
            }
            else if (Lines == 1)
            {
                Lesson.CurrentText = Lesson.CurrentText.Trim();
                Lesson.NextText = "";
                Lesson.LastText = "";
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

        private bool CanChangeLanguageCommandExecute(object parameter) => !IsStarted;

        private void OnChangeLanguageCommandExecuted(object parameter)
        {
            Language = parameter.ToString();
            Lesson.Source = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", $"{Language}.txt");
            OnCancelTestCommandExecuted(parameter);
            OnGenerateTextCommandExecuted(parameter);
        }
        #endregion

        #region Нажатие клавиш

        int[] lengths = new int[2];
        int symbolCount;
        int errorCount = 0;

        public ICommand KeyPressedCommand { get; }

        private void OnKeyPressedCommandExecuted(object parameter)
        {
            if (!isStarted)
                StartTest();

            if (TypedText == Lesson.CurrentText.Substring(0, TypedText.Length))
            {
                CurrentColor = "#DBDBDB";
            }
            else
            {
                CurrentColor = "#FF6323";
                bool isBack = CalculateTypedLengthDifference();
                if (TypedText[CurrentIndex - 1] != Lesson.CurrentText[CurrentIndex - 1] && isBack)
                {
                    errorCount++;
                }
            }

            if (TypedText.Length == Lesson.CurrentText.Length)
            {
                DoneLines++;
                symbolCount += Lesson.CurrentText.Length;

                if (DoneLines == Lines)
                {
                    EndTest(parameter);
                }

                TextSwap(parameter);
                lengths[0] = 0;
                lengths[1] = 0;
            }
        }

        bool CalculateTypedLengthDifference()
        {
            if (lengths[0] == 0)
            {
                lengths[0] = TypedText.Length;
            }
            else
            {
                lengths[1] = lengths[0];
                lengths[0] = TypedText.Length;
            }

            return lengths[0] >= lengths[1];
        }

        void StartTest()
        {
            DoneLines = 0;
            IsStarted = true;
            timer.Start();
        }

        void EndTest(object parameter)
        {
            timer.Stop();
            System.Windows.MessageBox.Show(
                "Вы успешно завершили тест, со статистикой вы можете ознакомиться по нажатию кнопки 'статистика'",
                "Тест успешно пройден",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            CalculateLessonStatsAndAddToFile();
            TypedText = "";
            ElapsedTime = 0;
            isStarted = false;
        }

        void CalculateLessonStatsAndAddToFile()
        {
            double accuracy = (1 - ((double)errorCount / (double)symbolCount)) * 100 >= 0 ? (1 - ((double)errorCount / (double)symbolCount)) * 100 : 0;
            string stats = $"Язык: {Language} | Введено строк: {Lines} | Количество ошибок: {errorCount} | Время: {ElapsedTime} секунд | Скорость ввода: {Math.Round(symbolCount / (ElapsedTime / 60), 2)} символов в минуту | Точность {Math.Round(accuracy)}%";
            string newEntry = $"{DateTime.Now}: {stats}\n";
            string existingContent = File.ReadAllText(statsFilePath);
            string newContent = newEntry + existingContent;
            File.WriteAllText(statsFilePath, newContent);
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
            TypedText = "";
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

        #region Открыть справку

        public ICommand OpenHelpCenterCommand { get; }

        void OnOpenHelpCenterCommandExecuted(object parameter)
        {
            Help.ShowHelp(null, "help.chm");
        }

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
            Lines = 3;
            DoneLines = 0;
            Lesson = new LessonText(Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", "russian.txt"), 77);

            #region Команды
            ChangeLanguageCommand = new RelayCommand(OnChangeLanguageCommandExecuted, CanChangeLanguageCommandExecute);
            GenerateTextCommand = new RelayCommand(OnGenerateTextCommandExecuted, CanGenerateTextCommandExecute);
            KeyPressedCommand = new RelayCommand(OnKeyPressedCommandExecuted);
            CancelTestCommand = new RelayCommand(OnCancelTestCommandExecuted, CanCancelTestCommandExecute);
            CloseApplicationCommand = new RelayCommand(OnCloseApplicationCommandExecute);
            MinimizeWindowCommand = new RelayCommand(OnMinimizeWindowCommandExecute);
            MaximizeWindowCommand = new RelayCommand(OnMaximizeWindowCommandExecute);
            OpenStatsFileCommand = new RelayCommand(OnOpenStatsFileCommandExecuted, CanOpenStatsFileCommandExecute);
            ClearStatsFileCommand = new RelayCommand(OnClearStatsFileCommandExecuted, CanClearStatsFileCommandExecute);
            OpenHelpCenterCommand = new RelayCommand(OnOpenHelpCenterCommandExecuted);
            #endregion
        }
    }
}
