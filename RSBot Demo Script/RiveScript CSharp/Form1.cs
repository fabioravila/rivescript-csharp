using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RiveScript;
using RiveScript_CSharp;

namespace RiveScript_CSharp
{
    public partial class Form1 : Form
    {
        string Brain = AppDomain.CurrentDomain.BaseDirectory + @"brain.rive";

        public Form1()
        {
            InitializeComponent();

            
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var bot = new RiveScript.RiveScript(true);
                try
                {
                var result = bot.loadFile(Brain);
                    Console.Write("Brain Has Been Loaded!");

                }
                catch
                {
                    MessageBox.Show("An Error has Occurred While Loading Brain!");
                    Console.Write("An Error has Occurred While Loading Brain!");
                }
                bot.sortReplies();


                textBox2.AppendText("Bot: " + bot.reply("local-user", textBox1.Text) + Environment.NewLine);
                textBox2.AppendText(Environment.NewLine);
                textBox2.AppendText("You: " + textBox1.Text + Environment.NewLine);
                textBox1.Text = "";
            }
        }
    }
}
