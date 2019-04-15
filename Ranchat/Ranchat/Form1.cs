using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Ranchat
{
    delegate void changeForm();
    public  partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public static string name;
        

        public Form1()
        {
            InitializeComponent();
            this.AcceptButton = metroButton1;
            
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {           
            if(nameText.Text == string.Empty)
            {
                MessageBox.Show("이름을 입력해주세요");
            }
            else if(hakbunText.Text == string.Empty)
            {
                MessageBox.Show("학번을 입력해주세요");
            }
            else
            {
                Program.Send(Program.mainSock, "login:" + nameText.Text + ":" + hakbunText.Text);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Class1.location = Location;
            if (Class1.form1_on)
            {
                Program.Send(Program.mainSock, "./disconnect");
                Program.mainSock.Close();
                Environment.Exit(0);
            }
            else
            {
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Location = Class1.location;
            new Thread(() =>
            {
                while (true)
                {
                    if (Class1.change)
                    {
                        Invoke(new Action(() =>
                        {
                            this.Close();
                        }));

                        Class1.change = false;
                        Form2 form2 = new Form2();
                        form2.ShowDialog();
                        break;
                    }
                }
            }).Start();
        }
    }
}
