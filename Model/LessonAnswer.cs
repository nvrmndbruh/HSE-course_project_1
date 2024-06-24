using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursach.Model
{
    public class LessonAnswer : BaseModel
    {
        private int doneLines;
        private int errorCount;
        private int symbolCount;
        private int currentIndex;
        private string typedText;
        private bool isStarted;

        public int DoneLines
        {
            get => doneLines;
            set
            {
                doneLines = value;
                OnPropertyChanged(nameof(DoneLines));
            }
        }

        public int ErrorCount
        {
            get => errorCount;
            set
            {
                errorCount = value;
                OnPropertyChanged(nameof(ErrorCount));
            }
        }

        public int SymbolCount
        {
            get => symbolCount;
            set
            {
                symbolCount = value;
                OnPropertyChanged(nameof(SymbolCount));
            }
        }

        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                currentIndex = TypedText.Length;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }

        public string TypedText
        {
            get => typedText;
            set
            {
                typedText = value;

                if (TypedText == null || TypedText == "")
                    CurrentIndex = 1;
                else
                    CurrentIndex = TypedText.Length - 1;

                OnPropertyChanged(nameof(TypedText));
            }
        }

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

        public LessonAnswer()
        {
            TypedText = "";
            DoneLines = 0;
            SymbolCount = 0;
            ErrorCount = 0;
            IsStarted = false;
        }
    }
}
