using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace combit.RedmineReports
{
    public partial class RedmineReportsForm : Form
    {
        string connString;
        RedmineMySqlDataAccess _dataAccess;
        public RedmineReportsForm()
        {
            try
            {
                InitializeComponent();

                dtpToDate.Text = DateTime.Now.ToShortDateString();
                dtpFromDate.Text = DateTime.Now.AddDays(-7).ToShortDateString();
                connString = ConfigurationManager.ConnectionStrings["combit.RedmineReports.Properties.Settings.RedmineConnectionString"].ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void btnDesign_Click(object sender, EventArgs e)
        {

            InitDataSource();
        }

        private void InitDataSource()
        {
            try
            {
                //read selected item
                DataRowView drView = (DataRowView)cmbProject.SelectedItem;
                string projectId = drView["id"].ToString();

                ListBox.SelectedIndexCollection listIndex = lboxVersion.SelectedIndices;
                StringBuilder sqlCommand = new StringBuilder();
/*
                if (listIndex.Count > 0)
                {
                    int i = 0;
                    foreach (int index in listIndex)
                    {
                        DataRowView drItem = (DataRowView)lboxVersion.Items[index];
                        if (i == 0)
                            sqlCommand.Append(" AND (issues.fixed_version_id = " + drItem["id"].ToString());
                        else
                            sqlCommand.Append(" OR issues.fixed_version_id = " + drItem["id"].ToString());
                        i++;
                    }
                    sqlCommand.Append(")");
                } 
                //get redmine project name
                _lL.Variables.Add("Redmine.ProjectName", _dataAccess.GetRedmineProjectName(projectId));

                // if more than one version is selected use "Multiple Versions"
                if (lboxVersion.SelectedIndices.Count == 1)
                {
                    DataRowView drItem = (DataRowView)lboxVersion.Items[lboxVersion.SelectedIndex];
                    _lL.Variables.Add("Redmine.VersionName", drItem["name"].ToString());
                }
                else if (lboxVersion.SelectedIndices.Count > 1)
                {
                    _lL.Variables.Add("Redmine.VersionName", "Multiple Versions");
                }
                else
                {
                    _lL.Variables.Add("Redmine.VersionName", String.Empty);
                }

                // get the redmine url
                _lL.Variables.Add("Redmine.HostName", _dataAccess.GetRedmineHostName());
                int startDate = Convert.ToInt32(tbStartDate.Text.ToString());
*/
                var datasource = _dataAccess.GetRedmineData(projectId, sqlCommand.ToString(), Convert.ToDateTime(dtpFromDate.Text), Convert.ToDateTime(dtpToDate.Text)); 
 
 }
            catch (DbException ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (RedmineReportsConfigDataHelper.ConnectionStringEncrypted(connString))
                {
                    _dataAccess = new RedmineMySqlDataAccess();
                }
                else
                {
                    if (RedmineReportsConfigDataHelper.ConnectionStringIsPlain(connString))
                    {
                        _dataAccess = new RedmineMySqlDataAccess(null, RedmineReportsConfigDataHelper.ConnectionStringIsPlain(connString));
                    }
                    else
                    {
                        //open config form for sql data
                        ConfigureMySqlDataBaseConnection();
                    }
                }

                // fill project combobox
                if (_dataAccess != null)
                    cmbProject.DataSource = _dataAccess.GetRedmineProjects(Convert.ToBoolean(ConfigurationManager.AppSettings["UseAllProjects"]));

                cmbProject.DisplayMember = "display_name";
                cmbProject.ValueMember = "id";

                // check or uncheck checkbox for subprojects
                cbAllProjects.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["UseAllProjects"]);

            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateVersionBox()
        {
            //read selected item
            if (cmbProject.SelectedItem != null)
            {
                DataRowView drView = (DataRowView)cmbProject.SelectedItem;
                string sProjectID = drView["id"].ToString();

                // get all versions for the project and fill the listbox
                lboxVersion.DataSource = _dataAccess.GetVersions(sProjectID); ;
                lboxVersion.DisplayMember = "name";
                lboxVersion.ValueMember = "id";
                lboxVersion.SelectedIndices.Clear();
                lboxVersion.SelectedIndex = lboxVersion.Items.Count - 1;
            }
            else
            {
                lboxVersion.DataSource = null;
            }
        }

        private void cmbProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateVersionBox();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dataAccess != null)
                _dataAccess.Dispose();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                    InitDataSource();
            }
            catch (DbException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbStartDate_TextChanged(object sender, EventArgs e)
        {
            TextBox source = sender as TextBox;
            if (source == null)
                return;

            string text = source.Text;
            if (Regex.IsMatch(text, "^[0-9]*$"))
                return;

            source.TextChanged -= this.tbStartDate_TextChanged;

            source.ResetText();
            if (source.TextLength != 1)
            {
                source.AppendText(text.Substring(0, text.Length - 1));
            }
            
            source.TextChanged += this.tbStartDate_TextChanged;
        }

        private void cbAllProjects_CheckedChanged(object sender, EventArgs e)
        {
            if (_dataAccess != null)
                cmbProject.DataSource = _dataAccess.GetRedmineProjects(cbAllProjects.Checked);
        }

        //private void tbFromDate_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (((Keys)e.KeyChar) == Keys.Back || ((int)e.KeyChar) == 46)
        //        return;
        //    if (!Regex.IsMatch(e.KeyChar.ToString(), "\\d+"))
        //        e.Handled = true;
        //}

        //private void tbToDate_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (((Keys)e.KeyChar) == Keys.Back || ((int)e.KeyChar) == 46)
        //        return;
        //    if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+"))
        //        e.Handled = true;
        //}

        private void rbTimespan_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDateRange.Checked)
            {
                dtpFromDate.Enabled = true;
                dtpToDate.Enabled = true;
            }
        }

        private void redmineDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RedmineMySqlConfig rmc = new RedmineMySqlConfig(this);
            rmc.Show();
        }

        public void reloadCmb(string ConnectionString)
        {
            //InitializeComponent();
            _dataAccess = null;
            _dataAccess = new RedmineMySqlDataAccess(ConnectionString);
            cmbProject.DataSource = null;
            cmbProject.DataSource = _dataAccess.GetRedmineProjects(Convert.ToBoolean(ConfigurationManager.AppSettings["UseAllProjects"]));
            cmbProject.DisplayMember = "display_name";
            cmbProject.ValueMember = "id";
        }

        private void ConfigureMySqlDataBaseConnection()
        {
            RedmineMySqlConfig rmc = new RedmineMySqlConfig(this);
            rmc.ShowDialog();
        }
    }
}