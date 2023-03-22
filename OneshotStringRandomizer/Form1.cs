using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace OneshotStringRandomizer
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void sendLog(string text, bool error = false)
        {
            this.richTextBox1.Select(this.richTextBox1.TextLength, 0);
            if (error) this.richTextBox1.SelectionColor = Color.Red;
            this.richTextBox1.AppendText("\r\n" + text);
            this.richTextBox1.SelectionColor = Color.Black;
        }

        CommonOpenFileDialog directoryDialog = new CommonOpenFileDialog();
        private void button1_Click(object sender, EventArgs e)
        {

            directoryDialog.InitialDirectory = this.textBox1.Text;
            directoryDialog.IsFolderPicker = true;
            if (directoryDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.textBox1.Text = directoryDialog.FileName;
            }
        }


        Random rng = new Random();
        private void button2_Click(object sender, EventArgs e)
        {
            Control[] controlElements = { this.textBox1, this.button1, this.checkBox1, this.checkBox2, this.button2 };
            foreach (Control obj in controlElements)
            {
                obj.Enabled = false;
            }
            string gameDirectory = this.textBox1.Text;
            bool preserveSpeechTags = this.checkBox1.Checked;
            bool randomizeImages = this.checkBox2.Checked;
            try
            {
                sendLog("Reading "+ gameDirectory + "\\Languages\\fr.po");
                string[] file = File.ReadAllLines(gameDirectory + "\\Languages\\fr.po");
                sendLog("Translating strings...");
                int li = -1;
                List<string> strings = new List<string>();
                foreach (string line in file)
                {
                    li++;
                    if (line.StartsWith("msgstr"))
                    {
                        file[li] = "msgstr " + file[li-1].Substring(6);
                        strings.Add(file[li - 1].Substring(6));
                    }
                }
                sendLog("Sorting strings...");
                List<string> sort_tags = new List<string>();
                List<string> sort_strings = new List<string>();
                foreach (string str in strings)
                {
                    //strip away quotation marks
                    string qs = str.Substring(1, str.Length - 2);
                    //idk why this is here but it might fix something. probably
                    qs = qs.Trim();
                    if (qs.StartsWith("@") && preserveSpeechTags)
                    {
                        //string has a speech tag, we need to split it
                        string speechTag = qs.Split(' ')[0];
                        sort_tags.Add(speechTag);
                        //this condition exists because theres this one (or maybe even more idfk) string that has a speech tag and nothing else
                        if (qs.Length == speechTag.Length) sort_strings.Add(qs.Substring(speechTag.Length));
                        else sort_strings.Add(qs.Substring(speechTag.Length + 1));
                    }
                    else
                    {
                        sort_tags.Add(null);
                        sort_strings.Add(qs);
                    }
                }
                sendLog("Shuffling strings...");
                sort_strings.Shuffle();
                sendLog("Updating translation file...");
                li = -1;
                int ti = -1;
                foreach (string line in file)
                {
                    li++;
                    if (line.StartsWith("msgstr"))
                    {
                        ti++;
                        if (sort_tags[ti] != null && preserveSpeechTags)
                        {
                            file[li] = "msgstr \"" + sort_tags[ti]+" " + sort_strings[ti]+"\"";
                        } else
                        {
                            file[li] = "msgstr \"" + sort_strings[ti] + "\"";
                        }
                    }
                }
                //hardcoded rewrites for language code
                file[5] = "msgid \"rand\"";
                file[6] = "msgstr \"rand\"";
                sendLog("Writing to " + gameDirectory + "\\Languages\\rand.po");
                File.WriteAllLines(gameDirectory + "\\Languages\\rand.po", file);
                sendLog("Updating language_fonts.ini");
                string[] langfonts = File.ReadAllLines(gameDirectory + "\\Languages\\language_fonts.ini");
                string font = "rand=Terminus (TTF)";
                if (Array.Find(langfonts, fontdef => fontdef == font) != font)
                {
                    File.AppendAllText(gameDirectory + "\\Languages\\language_fonts.ini", font);
                }
                //string randomization over, begin image randomization
                if (!randomizeImages) return;
                //theres probably an easier way to do these three but i dont care
                string[] introImages = {"felix", "instruction1", "instruction2", "instruction3", "instruction4"};
                string[] bookImages = { "clover", "dice", "fauna", "phosphor1", "phosphor2", "phosphor3", "phosphor4", "prophet", "prophetbot", "sketch"};
                string[] journalImages = { "c1", "c2", "c3", "c4", "c5", "c6", "c7", "final", "s1", "s2", "s3", "s4", "save", "t1", "t2", "t3", "t4", "t5", "t6", "t7", "t8", "t9", "t10", "t11", "t12", "t13", "t14", "t15", "t16" };
                //randomize intro images
                var shuffleIntro = new string[introImages.Length]; Array.Copy(introImages, shuffleIntro, introImages.Length); rng.Shuffle(shuffleIntro);
                int i = -1;
                if (!Directory.Exists(gameDirectory + "\\Graphics\\Pictures\\rand\\")) Directory.CreateDirectory(gameDirectory + "\\Graphics\\Pictures\\rand\\");
                foreach (string image in shuffleIntro)
                {
                    i++;
                    sendLog("Writing " + introImages[i] + " as " + shuffleIntro[i]);
                    File.Copy(gameDirectory + "\\Graphics\\Pictures\\" + introImages[i] + ".png", gameDirectory + "\\Graphics\\Pictures\\rand\\" + shuffleIntro[i] + ".png", true);
                }
                //randomize book images
                var shuffleBook = new string[bookImages.Length]; Array.Copy(bookImages, shuffleBook, bookImages.Length); rng.Shuffle(shuffleBook);
                i = -1;
                foreach (string image in shuffleBook)
                {
                    i++;
                    sendLog("Writing " + bookImages[i] + " as " + shuffleBook[i]);
                    File.Copy(gameDirectory + "\\Graphics\\Pictures\\book_" + bookImages[i] + ".png", gameDirectory + "\\Graphics\\Pictures\\rand\\book_" + shuffleBook[i] + ".png", true);
                }
                //randomize journal images
                var shuffleJournal = new string[journalImages.Length]; Array.Copy(journalImages, shuffleJournal, journalImages.Length); rng.Shuffle(shuffleJournal);
                i = -1;
                if (!Directory.Exists(gameDirectory + "\\Graphics\\Journal\\rand\\")) Directory.CreateDirectory(gameDirectory + "\\Graphics\\Journal\\rand\\");
                foreach (string image in shuffleJournal)
                {
                    i++;
                    sendLog("Writing " + journalImages[i] + " as " + shuffleJournal[i]);
                    File.Copy(gameDirectory + "\\Graphics\\Journal\\" + journalImages[i] + ".bmp", gameDirectory + "\\Graphics\\Journal\\rand\\" + shuffleJournal[i] + ".bmp", true);
                }
            } catch (Exception err)
            {
                sendLog(err.GetType().ToString()+": "+err.Message,true);
                sendLog(err.StackTrace,true);
            }
            sendLog("Randomization complete!");
            foreach (Control obj in controlElements)
            {
                obj.Enabled = true;
            }
        }
    }

    static class RandomExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
