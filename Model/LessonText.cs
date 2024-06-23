using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Kursach.Model
{
    public class LessonText : INotifyPropertyChanged
    {
        private Random random = new Random();

        private string currentText;
        private string nextText;
        private string lastText;
        private string source;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string CurrentText
        {
            get => currentText;
            set
            {
                currentText = value;
                OnPropertyChanged(nameof(CurrentText));
            }
        }

        public string NextText
        {
            get => nextText;
            set
            {
                nextText = value;
                OnPropertyChanged(nameof(NextText));
            }
        }

        public string LastText
        {
            get => lastText;
            set
            {
                lastText = value;
                OnPropertyChanged(nameof(LastText));
            }
        }

        public string Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public LessonText(string source, int lineLength = 100)
        {
            Source = source;
            CurrentText = GenerateText(Source, lineLength);
            NextText = GenerateText(Source, lineLength);
            LastText = GenerateText(Source, lineLength);
        }

        public string GenerateText(string source, int length = 100)
        {
            Random rnd = new Random(random.Next(1, 1000));
            string[] words = File.ReadAllLines(source);
            string text;
            do
            {
                text = "";
                do
                {
                    text += words[rnd.Next(words.Length)];
                    text += " ";
                } while (text.Length < length - 10);
            } while(text.Length > length);
            
            return text;
        }
    }
}
