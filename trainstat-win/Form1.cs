using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPA_gui2
{
    public partial class Form1 : Form
    {
        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public float Sum(params float[] customerssalary)
        {
            float result = 0;

            for (int i = 0; i < customerssalary.Length; i++)
            {
                result += customerssalary[i];
            }

            return result;
        }

        public float Average(params float[] customerssalary)
        {
            float sum = Sum(customerssalary);
            float result = sum / customerssalary.Length;
            return result;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Status";
            if(backgroundWorker1.IsBusy != true)
            {
                try
                {
                    backgroundWorker1.RunWorkerAsync();
                }
                catch(Exception ex)
                {
                    toolStripStatusLabel1.Text = "Wystąpił błąd: " + ex.Message;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        public void changeTitle(string title)
        {
            this.Text = title;
        }

        public class Status
        {
            public int code;
            public string message;
            public Status(int code2, string message2)
            {
                code = code2;
                message = message2;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;


            string content = Get("http://ipa.lovethosetrains.com/api/trains");
            dynamic pociagi = JObject.Parse(content);
            dynamic pociagi2 = pociagi.trains;
            //MessageBox.Show(pociagi2[0].train_name.Value);
            bool ok = false;
            int i = 0;
            int count = pociagi2.Count;
            string name;
            dynamic pociag;
            do
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                pociag = pociagi2[i];
                name = pociag["train_name"].Value;
                if (name.Contains(textBox1.Text.ToUpper()))
                {
                    ok = true;
                    if (checkBox1.Checked)
                    {
                        string dane = Get("http://ipa.lovethosetrains.com/api/trains/" + name.Replace(" ", "%20"));
                        dynamic pociag2 = JObject.Parse(dane);
                        if (pociag2["schedules"][0]["schedule_date"] != DateTime.Now.ToString("yyyy-MM-dd"))
                        {
                            ok = false;
                        }
                    }
                }
                i++;
                int n = (int)((float)((float)i / (float)count) * 100);
                worker.ReportProgress(n, new Status(0,""));
                if (n > 0)
                {
                    worker.ReportProgress(n-1, new Status(0, ""));
                    worker.ReportProgress(n, new Status(0, ""));
                }
                Console.WriteLine("chuj" + (float)((float)i / (float)count) * 100);
            } while (ok == false && i != count);
            worker.ReportProgress(0, new Status(0, ""));

            int id = i;

            //dataGridView1.Rows.Clear();

            //rows = new object[pociag["stations"].Count];
            object[] rows = new object[pociag["stations"].Count+1];
            if (id < count)
            {
                string dane = Get("http://ipa.lovethosetrains.com/api/trains/" + name.Replace(" ", "%20"));
                dynamic pociag2 = JObject.Parse(dane);
                //changeTitle("[" + id + "] " + name + " - IPA GUI 2");
                
                for (int j = 0; j < pociag["stations"].Count; j++)
                {
                    //MessageBox.Show("akurwinko");
                    string nazwa_stacji = pociag["stations"][j];
                    float[] stsr = new float[pociag2["schedules"].Count];
                    //MessageBox.Show("hujenkurwen");
                    for (int k = 0; k < pociag2["schedules"].Count; k++)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        //MessageBox.Show("chuj" + pociag["stations"].Count.ToString() + "kurwa" + j);
                        //if ((j / pociag["stations"].Count)*100 != 0)
                        //{
                            int progress=(int)(((float)j / (float)(pociag["stations"].Count-1)) * 100);

                        worker.ReportProgress(progress, new Status(0, ""));
                        if (progress > 0)
                        {
                            worker.ReportProgress(progress-1, new Status(0, ""));
                            worker.ReportProgress(progress, new Status(0, ""));
                        }
                        //MessageBox.Show(progress.ToString());
                        Console.WriteLine((float)j / (float)(pociag["stations"].Count-1));
                        //MessageBox.Show((((j / pociag["stations"].Count+0.01) * 100) / ((k / pociag2["schedules"].Count+0.01) * 100) * 100).toString());
                        //}
                        if (j == 0)
                        {
                            stsr[k] = pociag2["schedules"][k]["info"][j]["departure_delay"];
                        }
                        else
                        {
                            try
                            {
                                if (pociag["stations"][j] == pociag2["schedules"][k]["info"][j]["station_name"])
                                {
                                    if (pociag2["schedules"][k]["info"][j]["arrival_delay"] != null)
                                    {
                                        stsr[k] = pociag2["schedules"][k]["info"][j]["arrival_delay"];
                                    }
                                    else
                                    {
                                        stsr[k] = 0;
                                    }
                                }
                                else
                                {
                                    stsr[k] = 0;
                                }
                            }
                            catch(Exception ex)
                            {
                                worker.ReportProgress(progress, new Status(1, ex.Message));
                                stsr[k] = 0;
                            }
                            /*}
                            else
                            {
                                stsr[k] = 0;
                            }*/
                        }

                    }
                    //MessageBox.Show("dupa");

                    float sr = 0, max = 0, min = 2048;
                    string kiedy = "", kiedy2 = "";
                    for (int k = 0; k < stsr.Length; k++)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        sr = sr + stsr[k];
                        if (stsr[k] > max)
                        {
                            max = stsr[k];
                            if (j == 0)
                            {
                                kiedy = pociag2["schedules"][k]["info"][j]["departure_time"];
                            }
                            else
                            {
                                kiedy = pociag2["schedules"][k]["info"][j]["arrival_time"];
                            }
                        }
                        if (stsr[k] < min)
                        {
                            min = stsr[k];
                            if (j == 0)
                            {
                                kiedy2 = pociag2["schedules"][k]["info"][j]["departure_time"];
                            }
                            else
                            {
                                kiedy2 = pociag2["schedules"][k]["info"][j]["arrival_time"];
                            }
                        }
                    }
                    //MessageBox.Show("uuuu"+kiedy+"aaaa"+kiedy2);
                    DateTime d1= DateTime.Parse("2020-01-01T00:00:00", null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime d2 = DateTime.Parse("2020-01-01T00:00:00", null, System.Globalization.DateTimeStyles.RoundtripKind);
                    try
                    {
                        d1 = DateTime.Parse(kiedy2, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        d2 = DateTime.Parse(kiedy, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    catch(Exception ex)
                    {
                        //MessageBox.Show(ex.ToString(), "Wystąpił błąd podczas zbierania danych!");
                        int progress = (int)(((float)j / (float)(pociag["stations"].Count - 1)) * 100);
                        worker.ReportProgress(progress, new Status(1, ex.Message));
                    }
                    Object[] row = { j, nazwa_stacji, Math.Round(Average(stsr), 1), max, d2.ToString("dd.MM.yyyy"),  min, d1.ToString("dd.MM.yyyy") };
                    rows[j+1] = row;
                }
                rows[0]= new object[] { "[" + id + "] " + name };
                e.Result = rows;
            }
            else
            {
                e.Result = "NULL";
            }

            //Finish:
           

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Status status = (Status)e.UserState;
            if (status.code == 0)
            {
                progressBar1.Value = e.ProgressPercentage;
            }
            else
            {
                toolStripStatusLabel1.Text = "Wystąpił błąd: " + status.message;
            }
        }

        static bool isNot(int n)
        {
            return n != 0;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    MessageBox.Show("Anulowano");
                    progressBar1.Value = 0;
                }
                else
                {
                    dataGridView1.Rows.Clear();
                    if (e.Result.ToString() == "NULL")
                    {
                        MessageBox.Show("Nie znaleziono połączenia!");
                    }
                    else
                    {
                        object[] rows = (object[])e.Result;
                        object[] title = (object[])rows[0];


                        changeTitle(title[0].ToString() + " - TrainStat");

                        rows[0] = null;
                        foreach (object[] row in rows)
                        {
                            if (row != null)
                                dataGridView1.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(this,ex.ToString(),"Wystąpił błąd podczas zbierania danych!");
                toolStripStatusLabel1.Text = "Wystąpił błąd: " + ex.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void writeCSV(DataGridView gridIn, string outputFile)
        {
            //test to see if the DataGridView has any rows
            if (gridIn.RowCount > 0)
            {
                string value = "";
                DataGridViewRow dr = new DataGridViewRow();
                StreamWriter swOut = new StreamWriter(outputFile);

                //write header rows to csv
                for (int i = 0; i <= gridIn.Columns.Count - 1; i++)
                {
                    if (i > 0)
                    {
                        swOut.Write(",");
                    }
                    swOut.Write(gridIn.Columns[i].HeaderText);
                }

                swOut.WriteLine();

                //write DataGridView rows to csv
                for (int j = 0; j <= gridIn.Rows.Count - 1; j++)
                {
                    if (j > 0)
                    {
                        swOut.WriteLine();
                    }

                    dr = gridIn.Rows[j];

                    for (int i = 0; i <= gridIn.Columns.Count - 1; i++)
                    {
                        if (i > 0)
                        {
                            swOut.Write(",");
                        }

                        value = dr.Cells[i].Value.ToString();
                        //replace comma's with spaces
                        value = value.Replace(',', '.');
                        //replace embedded newlines with spaces
                        value = value.Replace(Environment.NewLine, " ");

                        swOut.Write(value);
                    }
                }
                swOut.Close();
            }
        }

        private void eksportujToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            writeCSV(dataGridView1, saveFileDialog1.FileName);
            MessageBox.Show("Pomyślnie zapisano plik! Oddzielanie: przecinki");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {

        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            new AboutBox1().Show();
        }

        private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
