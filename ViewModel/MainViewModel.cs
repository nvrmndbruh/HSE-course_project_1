using Kursach.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Kursach.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        #region Lesson

        LessonTask lesson;

        public LessonTask Lesson
        {
            get => lesson;
            set
            {
                lesson = value;
                OnPropertyChanged(nameof(Lesson));
            }
        }

        #endregion

        #region Answer

        LessonAnswer answer;

        public LessonAnswer Answer
        {
            get => answer;
            set
            {
                answer = value;
                OnPropertyChanged(nameof(Answer));
            }
        }

        #endregion

        #region Timer

        private TimerModel timer;
        public TimerModel Timer
        {
            get => timer;
            set
            {
                timer = value;
                OnPropertyChanged(nameof(Timer));
            }
        }

        #endregion

        readonly string statsFilePath = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources", "stats.txt");


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

        private int totalLines;
        public int TotalLines
        {
            get => totalLines;
            set
            {
                totalLines = value;
                OnPropertyChanged(nameof(TotalLines));
            }
        }

        public void TextSwap(object parameter)
        {
            Answer.TypedText = "";

            if ((Lesson.Lines - Answer.DoneLines) >= 3)
            {
                Lesson.CurrentText = Lesson.NextText;
                Lesson.NextText = Lesson.LastText;
                Lesson.LastText = Lesson.GenerateText(Lesson.Source, 77);
            }
            else if ((Lesson.Lines - Answer.DoneLines) == 2)
            {
                Lesson.CurrentText = Lesson.NextText;
                Lesson.NextText = Lesson.LastText;
                Lesson.LastText = "";
            }
            else if ((Lesson.Lines - Answer.DoneLines) == 1)
            {
                Lesson.CurrentText = Lesson.NextText.Trim();
                Lesson.NextText = "";
                Lesson.LastText = "";
            }
            
        }


        #region Команды

        #region Генерация текста

        public ICommand GenerateTextCommand { get; }

        private bool CanGenerateTextCommandExecute(object parameter) => !Answer.IsStarted;

        private void OnGenerateTextCommandExecuted(object parameter)
        {
            Answer = new LessonAnswer();
            Lesson = new LessonTask(Lesson.Source, TotalLines, 77);
            Timer = new TimerModel();

            Answer.IsStarted = false;
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

        private bool CanChangeLanguageCommandExecute(object parameter) => !Answer.IsStarted;

        private void OnChangeLanguageCommandExecuted(object parameter)
        {
            Language = parameter.ToString();
            Lesson.Source = Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", $"{Language}.txt");
            OnGenerateTextCommandExecuted(parameter);
        }
        #endregion

        #region Нажатие клавиш

        int[] lengths = new int[2];

        public ICommand KeyPressedCommand { get; }

        private void OnKeyPressedCommandExecuted(object parameter)
        {
            if (!Answer.IsStarted)
                StartTest();

            if (Answer.TypedText == Lesson.CurrentText.Substring(0, Answer.TypedText.Length))
            {
                CurrentColor = "#DBDBDB";
            }
            else
            {
                CurrentColor = "#FF6323";
                bool isBack = CalculateTypedLengthDifference();
                if (Answer.TypedText[Answer.CurrentIndex - 1] != Lesson.CurrentText[Answer.CurrentIndex - 1] && !isBack)
                {
                    Answer.ErrorCount++;
                }
            }

            if (Answer.TypedText.Length == Lesson.CurrentText.Length)
            {
                Answer.DoneLines++;
                Answer.SymbolCount += Lesson.CurrentText.Length;

                if (Answer.DoneLines == Lesson.Lines)
                {
                    EndTest(parameter);
                }
                else
                {
                    TextSwap(parameter);
                    lengths[0] = 0;
                    lengths[1] = 0;
                }
            }
        }

        bool CalculateTypedLengthDifference()
        {
            if (lengths[0] == 0)
            {
                lengths[0] = Answer.TypedText.Length;
            }
            else
            {
                lengths[1] = lengths[0];
                lengths[0] = Answer.TypedText.Length;
            }

            return lengths[0] < lengths[1];
        }

        void StartTest()
        {
            Answer.IsStarted = true;
            Timer.Start();
        }

        void EndTest(object parameter)
        {
            Timer.Stop();
            System.Windows.MessageBox.Show(
                "Вы успешно завершили тест, со статистикой вы можете ознакомиться по нажатию кнопки 'статистика'",
                "Тест успешно пройден",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            CalculateLessonStatsAndAddToFile();
            
            OnGenerateTextCommandExecuted(parameter);
        }

        void CalculateLessonStatsAndAddToFile()
        {
            double accuracy = (1 - ((double)Answer.ErrorCount / (double)Answer.SymbolCount)) >= 0 ? (1 - ((double)Answer.ErrorCount / (double)Answer.SymbolCount)) * 100 : 0;
            string stats = 
                $"Язык: {Language}" +
                $" | Введено строк: {Lesson.Lines}" +
                $" | Количество ошибок: {Answer.ErrorCount}" +
                $" | Время: {Timer.ElapsedTime} секунд" +
                $" | Скорость ввода: {Math.Round(Answer.SymbolCount / (Timer.ElapsedTime / 60), 2)} символов в минуту" +
                $" | Точность {Math.Round(accuracy)}%";

            string newEntry = $"{DateTime.Now}: {stats}\n";
            string existingContent = File.ReadAllText(statsFilePath);
            string newContent = newEntry + existingContent;

            File.WriteAllText(statsFilePath, newContent);
        }


        #endregion

        #region Отменить тест
        public ICommand CancelTestCommand { get; }

        private bool CanCancelTestCommandExecute(object parameter) => Answer.IsStarted;

        private void OnCancelTestCommandExecuted(object parameter)
        {
            Timer.Stop();
            Timer = new TimerModel();
            Answer = new LessonAnswer();
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

        bool CanOpenStatsFileCommandExecute(object parameter) => !Answer.IsStarted;

        #endregion

        #region Очистить файл со статистикой

        public ICommand ClearStatsFileCommand { get; }

        void OnClearStatsFileCommandExecuted(object parameter)
        {
            if (File.Exists(statsFilePath))
            {
                var answer = System.Windows.MessageBox.Show("Вы уверены, что хотите стереть все данные из файла статистики? После удаления данный не возможно будет восстановить.",
                    "Удалить статистику?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                    File.WriteAllText(statsFilePath, string.Empty);
            }
        }

        bool CanClearStatsFileCommandExecute(object parameter) => !Answer.IsStarted;

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


            Timer = new TimerModel();

            CurrentColor = "#DBDBDB";
            Language = "russian";

            

            TotalLines = 3;

            Lesson = new LessonTask(Path.Combine(Directory.GetParent((AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Resources/languages", "russian.txt"), TotalLines, 77);
            Answer = new LessonAnswer();


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
