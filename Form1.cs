using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// 1. file select
// 2. open and read
// 3. Grid header gen
// 4. Record append
// 5. Input to Cell

namespace DBManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void mnuMigration_Click(object sender, EventArgs e)
        {
            DialogResult ret = openFileDialog1.ShowDialog();
            if (ret != DialogResult.OK) return;
            string nFile = openFileDialog1.FileName;    // full name

            StreamReader sr = new StreamReader(nFile);

            // ==================================================
            //      Header 처리 프로세스
            // ==================================================
            string buf = sr.ReadLine();     // 1 Line Read
            if (buf == null) return;                
            string[] sArr = buf.Split(',');
            for(int i=0;i<sArr.Length;i++)
            {
                dataGrid.Columns.Add(sArr[i], sArr[i]);
            }

            // ==================================================
            //      Row 데이터 처리 프로세스
            // ==================================================
            while(true)
            {
                buf = sr.ReadLine();
                if (buf == null) break;
                sArr = buf.Split(',');
                //dataGrid.Rows.Add(sArr);
                int rIdx = dataGrid.Rows.Add(); // 1 Line 생성
                for (int i = 0; i < sArr.Length; i++)
                {
                    dataGrid.Rows[rIdx].Cells[i].Value = sArr[i];
                }
            }
        }
        SqlConnection sqlCon = new SqlConnection();
        SqlCommand sqlCmd = new SqlCommand();
        string sConn = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=;Integrated Security=True;Connect Timeout=30";

        private void mnuDBOpen_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult ret = openFileDialog1.ShowDialog();    // db file
                if (ret != DialogResult.OK) return;
                string nFile = openFileDialog1.FileName;    // full name
                string[] ss = sConn.Split(';');

                sqlCmd.Connection = sqlCon;
                sqlCon.ConnectionString = $"{ss[0]};{ss[1]}{nFile};{ss[2]};{ss[3]}";
                sqlCon.Open();
                sbPanel1.Text = openFileDialog1.SafeFileName;
                sbPanel2.Text = "Database opened success.";
                sbPanel1.BackColor = Color.Green;
            }
            catch(SqlException e1)
            {
                MessageBox.Show(e1.Message);
                sbPanel2.Text = "Database Cannot open.";
                sbPanel2.BackColor = Color.Red;
            }
        }
        public string GetToKen(int index, char deli, string str)
        {
            string[] Strs = str.Split(deli);
            string ret = Strs[index];
            return ret;
        }

        string TableName;   // 다른 메뉴에서 사용할 DB Table 이름
        int RunSql(string s1)
        {
            try
            {
                string sql = s1.Trim();
                sqlCmd.CommandText = sql;   // insert into fstatus values (1,2,3,4)
                if (GetToKen(0, ' ', sql).ToUpper() == "SELECT")
                {
                    SqlDataReader sr = sqlCmd.ExecuteReader();

                    TableName = GetToKen(3, ' ', sql);
                    dataGrid.Rows.Clear();
                    dataGrid.Columns.Clear();

                    for (int i = 0; i < sr.FieldCount; i++) // Header 처리
                    {
                        string ss = sr.GetName(i);
                        dataGrid.Columns.Add(ss, ss);
                    }
                    for (int i = 0; sr.Read(); i++)  //   1 record read : 1 줄
                    {
                        int rIdx = dataGrid.Rows.Add();    // 아무런 argument를 넣어주지 않으면 한줄이 생성됌
                        for (int j = 0; j < sr.FieldCount; j++)
                        {
                            object str = sr.GetValue(j);
                            dataGrid.Rows[rIdx].Cells[j].Value = str;
                        }
                    }
                    sr.Close();
                    //for (int i=0;sr.Read();i++)  //   1 record read : 1 줄
                    //{
                    //    string buf = "";
                    //    for (int j=0;j<sr.FieldCount;j++)
                    //    {
                    //        object str = sr.GetValue(j);
                    //        buf += $" {str} ";
                    //    }
                    //    tbSql.Text += $"\r\n{buf}";
                    //}
                    //sr.Close();
                }
                else
                {
                    sqlCmd.ExecuteNonQuery();   // select 문 제외 -- no return value
                                                // update, insert, delete, create, alt
                }
                sbPanel2.Text = "Success";
                sbPanel2.BackColor = Color.AliceBlue;
            }
            catch (SqlException e1)
            {
                MessageBox.Show(e1.Message);
                sbPanel2.Text = "Error";
                sbPanel2.BackColor = Color.Red;
            }
            catch(InvalidOperationException e2)
            {
                MessageBox.Show(e2.Message);
                sbPanel2.Text = "Error";
                sbPanel2.BackColor = Color.Red;
            }
            return 0;
        }

        private void mnuExecSql_Click(object sender, EventArgs e)
        {
            RunSql(tbSql.Text);
           
        }

        private void mnuSelSql_Click(object sender, EventArgs e)
        {
            RunSql(tbSql.SelectedText);
            //string sql = tbSql.SelectedText;
            //sqlCmd.CommandText = sql;
            //sqlCmd.ExecuteNonQuery();
        }

        private void tbSql_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            
            string str = tbSql.Text;
            string[] sArr = str.Split('\n');
            int n = sArr.Length;
            string sql = sArr[n - 1].Trim();
            RunSql(sql);
        }

        private void dataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = ".";    // 해당하는 cell을 가리킴
        }

        private void mnuUpdate_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGrid.Rows.Count;i++)
            {
                for(int j=0;j<dataGrid.Columns.Count;j++)
                {
                    string s = dataGrid.Rows[i].Cells[j].ToolTipText;    // 해당하는 cell을 가리킴
                    if(s == ".")
// update [Table]   set [field]=(CellText) where [keyName]=(Key.CellText)
// update [fStatus] set [Temp]=(10)        where [ID]     =(6)
                    {
                        string tn = TableName;
                        string fn = dataGrid.Columns[j].HeaderText;
                        string ct = (string)dataGrid.Rows[i].Cells[j].Value;
                        string kn = dataGrid.Columns[0].HeaderText;
                        int kt = (int)dataGrid.Rows[i].Cells[0].Value;
                        string sql = $"update {tn} set {fn}={ct} where {kn}={kt}";
                        RunSql(sql);
                    }
                }
            }
        }
    }
}
