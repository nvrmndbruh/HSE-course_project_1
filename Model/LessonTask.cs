using System;
using System.IO;

namespace Kursach.Model
{
    public class LessonTask : BaseModel
    {
        Random random = new Random();

        string currentText;
        string nextText;
        string lastText;
        int lines;

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

        string source;

        public string Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public int Lines
        {
            get => lines;
            set
            {
                lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        public LessonTask(string source, int lineCount, int lineLength = 100)
        {
            Source = source;
            Lines = lineCount;

            CurrentText = GenerateText(Source, lineLength);
            NextText = GenerateText(Source, lineLength);
            LastText = GenerateText(Source, lineLength);

            if (Lines == 2)
            {
                LastText = "";
            }
            else if (Lines == 1)
            {
                CurrentText = CurrentText.Trim();
                NextText = "";
                LastText = "";
            }
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

        public void TextSwap(LessonAnswer Answer)
        {
            Answer.TypedText = "";

            if ((Lines - Answer.DoneLines) >= 3)
            {
                CurrentText = NextText;
                NextText = LastText;
                LastText = GenerateText(Source, 77);
            }
            else if ((Lines - Answer.DoneLines) == 2)
            {
                CurrentText = NextText;
                NextText = LastText;
                LastText = "";
            }
            else if ((Lines - Answer.DoneLines) == 1)
            {
                CurrentText = NextText.Trim();
                NextText = "";
                LastText = "";
            }
        }
    }
}
