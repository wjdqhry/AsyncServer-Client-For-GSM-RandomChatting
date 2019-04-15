using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ranchat
{
    public partial class Form3 : MetroFramework.Forms.MetroForm
    {
        Thread exit_Check;
        Thread chat;
        public static string msg = string.Empty;

        public Form3()
        {
            InitializeComponent();
            this.AcceptButton = metroButton1;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Location = Class1.location;
            label2.Text = Class1.nickname_ + "님과 연결되었습니다. 대화를 나눠보세요";
            chat = new Thread(() =>
            {
                while (true)
                {
                    if(msg != string.Empty)
                    {
                        Invoke(new Action(() =>
                        {
                            richTextBox1.AppendText(Class1.nickname_ + ": " + msg + "\n");
                            richTextBox1.ScrollToCaret();
                        }));
                        msg = string.Empty;
                    }
                }
            });
            chat.Start();
            exit_Check = new Thread(() => exit_check());
            exit_Check.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Close();            
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if(metroTextBox2.Text != string.Empty)
            {
                richTextBox1.AppendText("나: " + metroTextBox2.Text + "\n");
                richTextBox1.ScrollToCaret();
                Program.Send(Program.mainSock, metroTextBox2.Text);
                metroTextBox2.Text = string.Empty;
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Class1.location = Location;
            exit_Check.Abort();
            chat.Abort();
            Class1.endConnect = false;
            Class1.connect = false;
            Program.Send(Program.mainSock, "./exit");
            Visible = false;
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        void exit_check()
        {
            while (true)
            {
                if (Class1.endConnect)
                {
                    Invoke(new Action(() =>
                    {
                        richTextBox1.ForeColor = Color.Red;
                        richTextBox1.AppendText(Class1.nickname_ + "님이 나가셨습니다....\n");
                        richTextBox1.ScrollToCaret();
                        metroButton1.Enabled = false;
                    }));
                    break;
                }
            }
        }
    }
}
