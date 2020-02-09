using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinatoPad14
{
    public partial class Form1 : Form
    {
        private bool dirtyFlag = false;
        private bool readOnlyFlag = false;

        Form2 fm2;
        AboutBox1 ab;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public string[] LoadList, txtBuff;
        public int txtstart, txtlength, txtp;
        public bool fInsertMode;

        string textmae, texthenkou, strfilename, access;
        int findStartIndex;
        OpenFileDialog ofd;
        SaveFileDialog sfd;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            texthenkou = textBox1.Text;
            if (textmae != texthenkou)
            {
                DialogResult result = new DialogResult();
                result = MessageBox.Show("ファイル " + strfilename + " の変更を保存しますか？", "MinatoPad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    if (strfilename == "無題")
                    {
                        sfd = new SaveFileDialog();
                        sfd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
                        sfd.FilterIndex = 1;
                        sfd.FileName = "*.txt";
                        DialogResult sfdres = new DialogResult();
                        sfdres = sfd.ShowDialog();

                        if (sfdres == DialogResult.OK)
                        {
                            StreamWriter writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.GetEncoding("Shift-JIS"));
                            writer.Write(textBox1.Text);
                            writer.Close();
                            textmae = textBox1.Text;
                            strfilename = Path.GetFileName(sfd.FileName);
                            access = Path.GetFullPath(sfd.FileName);
                            this.Text = strfilename + " - MinatoPad";
                            toolStripStatusLabel1.Text = "ファイルを保存: " + access;
                        }
                        else if (sfdres == DialogResult.Cancel)
                        {
                            e.Cancel = true;
                        }

                    }
                    else
                    {
                        Val_SaveFile(null, EventArgs.Empty);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
            else
            {
                return;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);
            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                textBox1.Text = System.IO.File.ReadAllText(cmds[1], System.Text.Encoding.Default);
                textmae = textBox1.Text;
                strfilename = Path.GetFileName(cmds[1]);
                access = Path.GetFullPath(cmds[1]);
                this.Text = strfilename + " - MinatoPad";
                toolStripStatusLabel1.Text = "ファイルを開く: " + access;
            }
            else
            {
                LoadList = new string[10];
                strfilename = "無題";
                this.Text = strfilename + " - MinatoPad";
                textmae = "";
            }
            texthenkou = "";
            findStartIndex = 0;
            textBox1.SelectionStart = 0;
            txtstart = 0;
            txtlength = 0;
            toolStripStatusLabel3.Text = "shift_jis";
            fInsertMode = true;
            toolStripStatusLabel2.Text = "挿入";
            getcurrentcursor();
            toolStripStatusLabel1.Text = "準備完了";
        }

        private void 新規作成NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string MSG_BOX_TITLE = "新しいファイルの作成";
            if (confirmDestructionText(MSG_BOX_TITLE))
            {
                strfilename = "無題";
                this.Text = strfilename + " - MinatoPad";
                textBox1.Clear();
                strfilename = "";
                setDirty(false);
            }
        }

        private void 開くOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_OpenFile(null, EventArgs.Empty);
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string MSGBOX_TITLE = "上書き保存";

            if (File.Exists(access))
            {
                try
                {
                    Val_SaveFile(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                Val_SaveFileAs(null, EventArgs.Empty);
            }
            //if (strfilename == "無題")
           //     Val_SaveFileAs(null, EventArgs.Empty);

           // else
           //     Val_SaveFile(null, EventArgs.Empty);
        }

        private void 名前を付けて保存AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_SaveFileAs(null, EventArgs.Empty);
        }

        private void ページ設定UToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 印刷PToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void minatoPadを終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 元に戻すUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.CanUndo == true)
            {
                textBox1.Undo();
                textBox1.ClearUndo();
            }
        }

        private void 切り取りTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectionLength > 0)
            {
                textBox1.Cut();
                toolStripStatusLabel1.Text = "Cut: " + Clipboard.GetText();
            }
        }

        private void コピーCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectionLength > 0)
            {
                textBox1.Copy();
                toolStripStatusLabel1.Text = "Copy: " + Clipboard.GetText();
            }
        }

        private void 貼り付けPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            if (data != null && data.GetDataPresent(DataFormats.Text))
            {
                textBox1.Paste();
                toolStripStatusLabel1.Text = "Paste: " + Clipboard.GetText();
            }
        }

        private void 削除LToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 検索と置換FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (検索と置換FToolStripMenuItem.Checked == true)
            {
                toolStrip1.Visible = false;
                textBox1.Dock = DockStyle.Fill;
                検索と置換FToolStripMenuItem.Checked = false;
            }
            else
            {
                toolStrip1.Visible = true;
                textBox1.Dock = DockStyle.Fill;
                検索と置換FToolStripMenuItem.Checked = true;
            }
        }

        private void 次を検索NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void 全て選択AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text;
            int textlong;
            text = textBox1.Text;
            textlong = int.Parse(text.Length.ToString());
            textBox1.Select(0, textlong);
        }

        private void 日付と時刻DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text, len;

            int mojilong, lenlong;
            text = textBox1.Text;
            mojilong = int.Parse(textBox1.SelectionStart.ToString());
            len = DateTime.Now.ToShortTimeString() + " " + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
            lenlong = int.Parse(len.Length.ToString());
            textBox1.Text = text.Insert(textBox1.SelectionStart, len);
            textBox1.SelectionStart = mojilong + lenlong;
            textBox1.SelectionLength = 0;
        }

        private void 右端で折り返すWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.txtfldwrdwrp == false)
            {
                Properties.Settings.Default.WordWrapChecked = true;
                Properties.Settings.Default.txtfldwrdwrp = true;
                右端で折り返すWToolStripMenuItem.Checked = true;
                toolStripStatusLabel1.Text = "設定: 右端で折り返す";
            }
            else
            {
                Properties.Settings.Default.WordWrapChecked = false;
                Properties.Settings.Default.txtfldwrdwrp = false;
                右端で折り返すWToolStripMenuItem.Checked = false;
                toolStripStatusLabel1.Text = "設定: 右端で折り返さない";
            }
        }

        private void フォントFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.TextBoxFont = fontDialog1.Font;
            }
        }

        private void 文字色CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                Properties.Settings.Default.TextBoxForeColor = colorDialog1.Color;
        }

        private void 背景色BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                Properties.Settings.Default.TextBoxBackColor = colorDialog1.Color;
        }

        private void hREFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(31, "<a href=" + @"""" + @"""" + " target=" + @"""" + "_blank" + @"""" + "></a>");
        }

        private void iMGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(28, "<img src=" + @"""" + @"""" + " align=" + @"""" + "right" + @"""" + " />");
        }

        private void 段落PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<p></p>");
        }

        private void 改行BRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(6, "<br />");
        }

        private void 区切線HRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(6, "<hr />");
        }

        private void 太字BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<b></b>");
        }

        private void 斜体IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<i></i>");
        }

        private void 下線UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<u></u>");
        }

        private void 取消線SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<s></s>");
        }

        private void 強調EMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(9, "<em></em>");
        }

        private void 強調STRONGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(17, "<strong></strong>");
        }

        private void 挿入INSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(11, "<ins></ins>");
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Val_Insert(11, "<sup></sup>");
        }

        private void 下添字SUBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(11, "<sub></sub>");
        }

        private void 引用QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(7, "<q></q>");
        }

        private void 引用BLOCKQUOTEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(25, "<blockquote></blockquote>");
        }

        private void リストの読み込みRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader srText = new StreamReader(ofd.FileName, System.Text.Encoding.Default);


                LoadList[0] = srText.ReadLine();
                LoadList[1] = srText.ReadLine();
                LoadList[2] = srText.ReadLine();
                LoadList[3] = srText.ReadLine();
                LoadList[4] = srText.ReadLine();
                LoadList[5] = srText.ReadLine();
                LoadList[6] = srText.ReadLine();
                LoadList[7] = srText.ReadLine();
                LoadList[8] = srText.ReadLine();
                LoadList[9] = srText.ReadLine();

                toolStripMenuItem3.Text = "1. " + LoadList[0];
                toolStripMenuItem4.Text = "2. " + LoadList[1];
                toolStripMenuItem5.Text = "3. " + LoadList[2];
                toolStripMenuItem6.Text = "4. " + LoadList[3];
                toolStripMenuItem7.Text = "5. " + LoadList[4];
                toolStripMenuItem8.Text = "6. " + LoadList[5];
                toolStripMenuItem9.Text = "7. " + LoadList[6];
                toolStripMenuItem10.Text = "8. " + LoadList[7];
                toolStripMenuItem11.Text = "9. " + LoadList[8];
                toolStripMenuItem12.Text = "0. " + LoadList[9];
            }
        }

        private void リストの編集EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fm2 = new Form2();
            fm2.fm1 = this;
            fm2.ShowDialog();
            fm2.Dispose();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(0);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(1);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(2);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(3);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(4);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(5);
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(6);
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(7);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(8);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Val_GetKeyIns(9);
        }

        private void ステータスバーSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.stsrpvis == false)
            {
                Properties.Settings.Default.StatusBarChecked = true;
                ステータスバーSToolStripMenuItem.Checked = true;
                Properties.Settings.Default.stsrpvis = true;
            }
            else if (Properties.Settings.Default.stsrpvis == true)
            {
                Properties.Settings.Default.StatusBarChecked = false;
                ステータスバーSToolStripMenuItem.Checked = false;
                Properties.Settings.Default.stsrpvis = false;
            }
        }

        private void アクセシビリティを有効にするToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (アクセシビリティを有効にするToolStripMenuItem.Checked == false)
            {
                textBox1.ForeColor = Color.FromKnownColor(KnownColor.Window);
                textBox1.BackColor = Color.FromKnownColor(KnownColor.WindowText);
                statusStrip1.ForeColor = Color.FromKnownColor(KnownColor.Window);
                statusStrip1.BackColor = Color.FromKnownColor(KnownColor.WindowText);
                アクセシビリティを有効にするToolStripMenuItem.Checked = true;
                toolStripStatusLabel1.Text = "Enable Accessibility.";
            }
            else
            {
                textBox1.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
                textBox1.BackColor = Color.FromKnownColor(KnownColor.Window);
                statusStrip1.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
                statusStrip1.BackColor = Color.FromKnownColor(KnownColor.Window);
                アクセシビリティを有効にするToolStripMenuItem.Checked = false;
                toolStripStatusLabel1.Text = "Disable Accessibility.";
            }
        }

        private void readmetxtの表示HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // カレントディレクトリを取得する
            string stCurrentDir = System.IO.Directory.GetCurrentDirectory();
            Process.Start(stCurrentDir + "/Readme.txt");
        }

        private void minatoPadについてAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ab = new AboutBox1();
            ab.ShowDialog();
            ab.Dispose();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string searchtxt, deftxt;
            int txtlen, foundIndex;
            searchtxt = toolStripTextBox1.Text;
            txtlen = toolStripTextBox1.Text.Length;
            deftxt = textBox1.Text;


            if (searchtxt == "")
            {
                toolStripStatusLabel1.Text = "エラー!!: 検索文字列を入力して下さい";
            }
            else
            {
                foundIndex = deftxt.LastIndexOf(searchtxt, findStartIndex);
                if (foundIndex == -1)
                {
                    toolStripStatusLabel1.Text = "検索完了";
                    findStartIndex = 0;
                    return;
                }
                else
                {
                    textBox1.SelectionStart = foundIndex;
                    textBox1.SelectionLength = txtlen;
                    findStartIndex = foundIndex - txtlen;
                    textBox1.ScrollToCaret();
                    toolStripStatusLabel1.Text = "次で検索: " + searchtxt;
                }

            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            string searchtxt, deftxt;
            int txtlen, foundIndex;
            searchtxt = toolStripTextBox1.Text;
            txtlen = toolStripTextBox1.Text.Length;
            deftxt = textBox1.Text;


            if (searchtxt == "")
            {
                toolStripStatusLabel1.Text = "エラー!!: 検索文字列を入力して下さい";
            }
            else
            {
                foundIndex = deftxt.IndexOf(searchtxt, findStartIndex);
                if (foundIndex == -1)
                {
                    toolStripStatusLabel1.Text = "検索完了";
                    findStartIndex = 0;
                    return;
                }
                else
                {
                    textBox1.SelectionStart = foundIndex;
                    textBox1.SelectionLength = txtlen;
                    findStartIndex = foundIndex + txtlen;
                    textBox1.ScrollToCaret();
                    toolStripStatusLabel1.Text = "次で検索: " + searchtxt;
                }

            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string selecttxt, reptxt;

            selecttxt = textBox1.SelectedText;
            reptxt = toolStripTextBox2.Text;

            if (selecttxt == "")
            {
                toolStripStatusLabel1.Text = "エラー!!: 文字列が選択されていません";
            }
            else
            {
                textBox1.SelectedText = reptxt;
                toolStripStatusLabel1.Text = "置換: " + selecttxt + " -> " + reptxt;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            string searchtxt, reptxt;
            searchtxt = toolStripTextBox1.Text;
            reptxt = toolStripTextBox2.Text;

            if (searchtxt == "")
            {
                toolStripStatusLabel1.Text = "エラー!!: 検索文字列を入力して下さい";
            }
            else
            {
                textBox1.Text = textBox1.Text.Replace(searchtxt, reptxt);
                toolStripStatusLabel1.Text = "全て置換: " + searchtxt + " -> " + reptxt;
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (toolStrip1.Dock == DockStyle.Top)
            {
            }
            else
            {
                toolStrip1.Dock = DockStyle.Top;
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (toolStrip1.Dock == DockStyle.Bottom)
            {
            }
            else
            {
                toolStrip1.Dock = DockStyle.Bottom;
            }
        }

        private void 削除DELToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Val_Insert(11, "<del></del>");
        }

        // Open File Function
        private string Val_OpenFile(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                const string MSGBOX_TITLE = "ファイル オープン";
                setDirty(false);
                try
                {
                    textBox1.Text = File.ReadAllText(ofd.FileName, System.Text.Encoding.Default);
                    textmae = textBox1.Text;
                    strfilename = Path.GetFileName(ofd.FileName);
                    access = Path.GetFullPath(ofd.FileName);
                    this.Text = strfilename + " - MinatoPad";
                    toolStripStatusLabel1.Text = "ファイルを開く: " + access;
                }catch(Exception ex)
                {
                    MessageBox.Show(this, ex.Message, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }

            return strfilename;
        }

        // Save File as Name Function
        private string Val_SaveFileAs(object sender, EventArgs e)
        {
            sfd = new SaveFileDialog();
            sfd.Filter = "テキスト文書(*.txt)|*.txt|HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.FileName = "*.txt";
            DialogResult result = new DialogResult();
            result = sfd.ShowDialog();

            if (result == DialogResult.OK)
            {
                strfilename = Path.GetFileName(sfd.FileName);
                access = Path.GetFullPath(sfd.FileName);
                File.WriteAllText(access, textBox1.Text, Encoding.Default);
                textmae = textBox1.Text;
                this.Text = strfilename + " - MinatoPad";
                toolStripStatusLabel1.Text = "ファイルを保存: " + access;
            }
            else
            {
            }

            return strfilename;
        }

        // Save File Function
        private string Val_SaveFile(object sender, EventArgs e)
        {
            File.WriteAllText(access, textBox1.Text, Encoding.Default);
            textmae = textBox1.Text;
            toolStripStatusLabel1.Text = "ファイルを保存: " + access;
            setDirty(false);

            return strfilename;
        }

        // Insert Tag Function
        private long Val_Insert(int num, string text)
        {
            string instext;
            int mojilong;
            instext = textBox1.Text;
            mojilong = int.Parse(textBox1.SelectionStart.ToString());
            textBox1.Text = instext.Insert(textBox1.SelectionStart, text);
            textBox1.SelectionStart = mojilong + num;
            textBox1.SelectionLength = 0;
            toolStripStatusLabel1.Text = "挿入: " + text;

            return 0;
        }

        // Insert Key Function
        private long Val_GetKeyIns(int num)
        {
            try
            {
                string text, len;
                int mojilong, lenlong;
                text = textBox1.Text;
                mojilong = int.Parse(textBox1.SelectionStart.ToString());
                len = this.LoadList[num];
                lenlong = int.Parse(len.Length.ToString());
                textBox1.Text = text.Insert(textBox1.SelectionStart, len);
                textBox1.SelectionStart = mojilong + lenlong;
                textBox1.SelectionLength = 0;
                toolStripStatusLabel1.Text = "貼り付け: " + len;
            }
            catch (Exception)
            {
                toolStripStatusLabel1.Text = "貼り付けエラー!!: テキストが空白です";
            }

            return 0;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (GetAsyncKeyState(Keys.ControlKey) < 0)
            {
                if (GetAsyncKeyState(Keys.D1) < 0)
                    Val_GetKeyIns(0);
                else if (GetAsyncKeyState(Keys.D2) < 0)
                    Val_GetKeyIns(1);
                else if (GetAsyncKeyState(Keys.D3) < 0)
                    Val_GetKeyIns(2);
                else if (GetAsyncKeyState(Keys.D4) < 0)
                    Val_GetKeyIns(3);
                else if (GetAsyncKeyState(Keys.D5) < 0)
                    Val_GetKeyIns(4);
                else if (GetAsyncKeyState(Keys.D6) < 0)
                    Val_GetKeyIns(5);
                else if (GetAsyncKeyState(Keys.D7) < 0)
                    Val_GetKeyIns(6);
                else if (GetAsyncKeyState(Keys.D8) < 0)
                    Val_GetKeyIns(7);
                else if (GetAsyncKeyState(Keys.D9) < 0)
                    Val_GetKeyIns(8);
                else if (GetAsyncKeyState(Keys.D0) < 0)
                    Val_GetKeyIns(9);
                else
                {
                }

            }
            else if (GetAsyncKeyState(Keys.Insert) < 0)
            {
                fInsertMode = !fInsertMode;
                if (fInsertMode == true)
                {
                    toolStripStatusLabel2.Text = "挿入";
                }
                else
                {
                    toolStripStatusLabel2.Text = "上書き";
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (fInsertMode == false)
            {
                if (!(GetAsyncKeyState(Keys.Back) < 0) && !(GetAsyncKeyState(Keys.Delete) < 0))
                {
                    textBox1.SelectionLength = 1;
                }
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            getcurrentcursor();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            findStartIndex = textBox1.SelectionStart;
            txtstart = textBox1.SelectionStart;
            txtlength = textBox1.SelectionLength;
        }

        private void setDirty(bool flag)
        {
            dirtyFlag = flag;
            保存SToolStripMenuItem.Enabled = (readOnlyFlag) ? false : flag;
        }

        private bool confirmDestructionText(string msgboxTitle)
        {
            const string MSG_BOX_STRING = "ファイルは保存されていません。\n\n編集中のテキストは破棄されます。\n\nよろしいですか？";
            if (!dirtyFlag)
                return true;

            return (MessageBox.Show(MSG_BOX_STRING, msgboxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

        }

        private void getcurrentcursor()
        {
            string str = textBox1.Text;
            int selectPos = textBox1.SelectionStart;

            int row = 1;
            int startPos = 0;
            int len = 0;

            for(int endPos = 0; (endPos = str.IndexOf('\n', startPos)) < selectPos && endPos > -1; row++)
            {
                startPos = endPos + 1;
            }

            len = GetLengthInTextElements(textBox1.Text) - CountChar(textBox1.Text, "\r\n") - CountChar(textBox1.Text, "\t") - CountChar(textBox1.Text, " ") - CountChar(textBox1.Text, "　");

            int col = selectPos - startPos + 1;
            toolStripStatusLabel4.Text = row + " 行";
            toolStripStatusLabel5.Text = col + " 桁";
            toolStripStatusLabel6.Text = len.ToString() + " 字 / " + textBox1.MaxLength.ToString() + " 字";
           
        }

        static int GetLengthInTextElements(string textData)
        {
            int[] indexes =
              StringInfo.ParseCombiningCharacters(textData);

            return indexes.Length;
        }

        public static int CountChar(string s, string c)
        {
            return s.Length - s.Replace(c.ToString(), "").Length;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox1.Text = File.ReadAllText(fileName[0], System.Text.Encoding.Default);
            textmae = textBox1.Text;
            strfilename = Path.GetFileName(fileName[0]);
            access = Path.GetFullPath(fileName[0]);
            this.Text = strfilename + " - MinatoPad";
            toolStripStatusLabel1.Text = "ファイルを開く: " + access;
        }

        void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if (this.WindowState != FormWindowState.Normal)
            {
                if ((e.SettingName == "Form1Client") || (e.SettingName == "Form1Location"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            setDirty(true);
            
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            //textBox1.SelectionStart = txtstart;
            //textBox1.SelectionLength = txtlength;
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            getcurrentcursor();
        }

        private void sHIFTJISToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeencode("shift_jis");
        }

        private void jISToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeencode("iso-2022-jp");
        }

        private void eUCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeencode("euc-jp");
        }

        private void unicodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeencode("unicodeFFFE");
        }

        private void uTF8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeencode("utf-8");
        }

        private void changeencode(string enc)
        {
            try
            {
                textBox1.Text = System.IO.File.ReadAllText(access, System.Text.Encoding.GetEncoding(enc));
                toolStripStatusLabel1.Text = "エンコード: " + enc;
                toolStripStatusLabel3.Text = enc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }   
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            string target, subst;
            char c;
            int sels, len, count;
            target = textBox1.Text;
            sels = textBox1.SelectionStart;
            len = 1;
            count = 0;

            try
            {
                // 全てのテキストから選択する範囲を設定
                subst = target.Substring(sels, len);

                // 判定する文字
                c = subst[count];

                if (IsHiragana(c))
                {
                    len += 1;
                    count += 1;
                    subst = target.Substring(sels, len);
                    c = subst[count];

                    while (IsHiragana(c))
                    {
                        try
                        {
                            len += 1;
                            count += 1;
                            subst = target.Substring(sels, len);
                            c = subst[count];
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    }
                    textBox1.Select(textBox1.SelectionStart, len - 1);
                    toolStripStatusLabel1.Text = textBox1.SelectionLength + " 文字選択中";
                }
                else if (IsFullwidthKatakana(c))
                {
                    len += 1;
                    count += 1;
                    subst = target.Substring(sels, len);
                    c = subst[count];

                    while (IsFullwidthKatakana(c))
                    {
                        try
                        {
                            len += 1;
                            count += 1;
                            subst = target.Substring(sels, len);
                            c = subst[count];
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    }
                    textBox1.Select(textBox1.SelectionStart, len - 1);
                    toolStripStatusLabel1.Text = textBox1.SelectionLength + " 文字選択中";
                }
                else if (IsHalfwidthKatakana(c))
                {
                    len += 1;
                    count += 1;
                    subst = target.Substring(sels, len);
                    c = subst[count];

                    while (IsHalfwidthKatakana(c))
                    {
                        try
                        {
                            len += 1;
                            count += 1;
                            subst = target.Substring(sels, len);
                            c = subst[count];
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    }
                    textBox1.Select(textBox1.SelectionStart, len - 1);
                    toolStripStatusLabel1.Text = textBox1.SelectionLength + " 文字選択中";
                }
                else if (IsKanji(c))
                {
                    len += 1;
                    count += 1;
                    subst = target.Substring(sels, len);
                    c = subst[count];

                    while (IsKanji(c))
                    {
                        try
                        {
                            len += 1;
                            count += 1;
                            subst = target.Substring(sels, len);
                            c = subst[count];
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    }
                    textBox1.Select(textBox1.SelectionStart, len - 1);
                    toolStripStatusLabel1.Text = textBox1.SelectionLength + " 文字選択中";
                }
            }
            catch (Exception excep)
            {
                toolStripStatusLabel1.Text = "エラー!!: 文字列は選択されていません";
            }
            
        }

        private static bool IsHiragana(char c)
        {
            return ('\u3041' <= c && c <= '\u309F') || c == '\u30FC' || c == '\u30A0';
        }

        private static bool IsFullwidthKatakana(char c)
        {
            return ('\u30A0' <= c && c <= '\u30FF') || ('\u31F0' <= c && c <= '\u31FF') || ('\u3099' <= c && c <= '\u309C');
        }

        private static bool IsHalfwidthKatakana(char c)
        {
            return '\uFF65' <= c && c <= '\uFF9F';
        }

        private static bool IsKanji(char c)
        {
            return ('\u4E00' <= c && c <= '\u9FCF') || ('\uF900' <= c && c <= '\uFAFF') || ('\u3400' <= c && c <= '\u4DBF');
        }

        private static bool Latin(char c)
        {
            return ('A' <= c && c <= 'Z') || ('Ａ' <= c && c <= 'Ｚ') || ('a' <= c && c <= 'z') || ('ａ' <= c && c <= 'ｚ');
        }
    }
}
