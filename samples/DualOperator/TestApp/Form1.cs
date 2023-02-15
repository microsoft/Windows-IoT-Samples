using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TestApp.Models;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Get the Window Title from the apptitle.json file
            List<AppTitle> appList = JsonSerializer.Deserialize<List<AppTitle>>(File.ReadAllText("apptitle.json"));
            if (appList != null && appList.Count > 0)
            {
                this.Text = appList[0].ApplicationTitle;
            }
            else
            {
                this.Text = "WHAT?!";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get out!
            Application.Exit();
        }
    }
}