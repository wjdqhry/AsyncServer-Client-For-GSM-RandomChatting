using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ranchat
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            this.AcceptButton = button1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {                
                string result = Program.Connect(textBox1.Text);
                if(result == "성공")
                {
                    Visible = false;
                    Class1.location = Location;
                    Form1 form1 = new Form1();
                    form1.ShowDialog();
                }
                else
                {
                    MessageBox.Show("접속 실패\n" + result);
                    textBox1.Text = "";
                }
                
            }
        }
    }
}
