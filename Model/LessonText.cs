using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursach.Model
{
    public class LessonText
    {
        private Random random = new Random();

        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string Source { get; set; }

        public LessonText(string source, int lineLength = 100)
        {
            Source = source;
            Text1 = GenerateText(Source, lineLength);
            Text2 = GenerateText(Source, lineLength);
            Text3 = GenerateText(Source, lineLength);
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
        /*
                public override string ToString()
                {
                    string text = "";
                    foreach (string word in Text)
                    {
                        text += word;
                        text += " ";
                    }
                    return text.Trim();
                }*/
    }
}
