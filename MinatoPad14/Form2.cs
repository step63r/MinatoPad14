using System;
using System.IO;
using System.Windows.Forms;

namespace MinatoPad14
{
    public partial class Form2 : Form
    {
        public Form1 fm1;
        OpenFileDialog ofd;
        SaveFileDialog sfd;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader srText = new StreamReader(ofd.FileName, System.Text.Encoding.Default);

                textBox1.Text = srText.ReadLine();
                textBox2.Text = srText.ReadLine();
                textBox3.Text = srText.ReadLine();
                textBox4.Text = srText.ReadLine();
                textBox5.Text = srText.ReadLine();
                textBox6.Text = srText.ReadLine();
                textBox7.Text = srText.ReadLine();
                textBox8.Text = srText.ReadLine();
                textBox9.Text = srText.ReadLine();
                textBox10.Text = srText.ReadLine();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fm1.LoadList[0] = textBox1.Text;
            fm1.LoadList[1] = textBox2.Text;
            fm1.LoadList[2] = textBox3.Text;
            fm1.LoadList[3] = textBox4.Text;
            fm1.LoadList[4] = textBox5.Text;
            fm1.LoadList[5] = textBox6.Text;
            fm1.LoadList[6] = textBox7.Text;
            fm1.LoadList[7] = textBox8.Text;
            fm1.LoadList[8] = textBox9.Text;
            fm1.LoadList[9] = textBox10.Text;

            fm1.toolStripMenuItem3.Text = "1. " + fm1.LoadList[0];
            fm1.toolStripMenuItem4.Text = "2. " + fm1.LoadList[1];
            fm1.toolStripMenuItem5.Text = "3. " + fm1.LoadList[2];
            fm1.toolStripMenuItem6.Text = "4. " + fm1.LoadList[3];
            fm1.toolStripMenuItem7.Text = "5. " + fm1.LoadList[4];
            fm1.toolStripMenuItem8.Text = "6. " + fm1.LoadList[5];
            fm1.toolStripMenuItem9.Text = "7. " + fm1.LoadList[6];
            fm1.toolStripMenuItem10.Text = "8. " + fm1.LoadList[7];
            fm1.toolStripMenuItem11.Text = "9. " + fm1.LoadList[8];
            fm1.toolStripMenuItem12.Text = "0. " + fm1.LoadList[9];

            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sfd = new SaveFileDialog();
            sfd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.FileName = "*.txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter Writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.GetEncoding("Shift-JIS"));
                Writer.Write(textBox1.Text + System.Environment.NewLine + textBox2.Text + System.Environment.NewLine + textBox3.Text + System.Environment.NewLine + textBox4.Text + System.Environment.NewLine + textBox5.Text + System.Environment.NewLine + textBox6.Text + System.Environment.NewLine + textBox7.Text + System.Environment.NewLine + textBox8.Text + System.Environment.NewLine + textBox9.Text + System.Environment.NewLine + textBox10.Text);
                Writer.Close();
            }
        }
    }
}
