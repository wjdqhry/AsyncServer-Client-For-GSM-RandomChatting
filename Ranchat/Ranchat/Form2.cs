using MaterialSkin;
using MaterialSkin.Controls;
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
    public partial class Form2 : MetroFramework.Forms.MetroForm 
    {
        Thread check;        

        public Form2()
        {
            InitializeComponent();            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            metroTextBox1.Text = Class1.nickname;
            Class1.form1_on = false;
            check = new Thread(() =>
            {
                while (true)
                {
                    Program.Send(Program.mainSock, "./num");
                    Invoke(new Action(() => label3.Text = "현재인원: " + Class1.num + "명"));
                    Thread.Sleep(2000);
                }
            });
            check.Start();
            new Thread(() =>
            {
                while (true)
                {
                    if (Class1.connect)
                    {
                        Invoke(new Action(() =>
                        {
                            Close();
                        }));
                        Form3 form3 = new Form3();
                        form3.ShowDialog();
                        break;
                    }
                }
            }).Start();
            Location = Class1.location;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Class1.searching)
            {
                Class1.nickname = metroTextBox1.Text;
                button1.Text = "중지";
                Program.Send(Program.mainSock, "search:" + Class1.nickname);
                Class1.searching = true;
            }
            else
            {
                Program.Send(Program.mainSock, "./cancle");
                button1.Text = "상대 찾기";
                Class1.searching = false;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Class1.location = Location;
            check.Abort();
            Class1.form1_on = true;
            if (!Class1.connect)
            {
                Program.Send(Program.mainSock, "logout");
                this.Visible = false;
                Form1 form1 = new Form1();
                form1.ShowDialog();
            }
            
            this.Visible = false;            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

