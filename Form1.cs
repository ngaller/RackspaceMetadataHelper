using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RackspaceMetadataHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Events

        private void txtApiKey_TextChanged(object sender, EventArgs e)
        {
            RefreshContainerList();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtUsername_Leave(object sender, EventArgs e)
        {
            RefreshContainerList();
        }

        private void txtApiKey_Leave(object sender, EventArgs e)
        {

        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtApiKey.Text))
                MessageBox.Show(@"Please enter your credentials");
            else if (string.IsNullOrEmpty(cbxContainer.Text))
                MessageBox.Show(@"Please select container");
            else if (string.IsNullOrEmpty(txtHeader.Text) || !txtHeader.Text.Contains(":"))
                MessageBox.Show(@"Please enter header and value in key: value format");
            else
            {
                var p = new RequestParams
                {
                    ApiKey = txtApiKey.Text,
                    UserName = txtUsername.Text,
                    ContainerName = cbxContainer.Text,
                    KeyValue = txtHeader.Text.Split(new char[] { ':' }, 2)
                };
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += Bw_DoWork;
                bw.ProgressChanged += Bw_ProgressChanged;
                bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                progressBar1.Visible = true;
                btnApply.Enabled = false;
                bw.RunWorkerAsync(p);
            }
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;
            var p = (RequestParams)e.Argument;
            try
            {
                var count = ApiHelper.ApplyObjectHeader(p.UserName, p.ApiKey, p.ContainerName, p.KeyValue[0],
                    p.KeyValue[1], (int current, int total) =>
                    {
                        bw.ReportProgress((current * 100)/ total);
                    });
                e.Result = count;
            }
            catch (Exception x)
            {
                e.Result = x;
            }
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            btnApply.Enabled = true;
            var x = e.Result as Exception;
            if (x != null)
            {
                MessageBox.Show($"There was an error applying the headers: {x.Message}");
            }
            else
            {
                MessageBox.Show($"Header has been applied to {e.Result} objects in container");
            }
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        #endregion

        #region Helpers

        private void RefreshContainerList()
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtApiKey.Text))
                return;

            try
            {
                var conts = ApiHelper.ListContainers(txtUsername.Text, txtApiKey.Text);
                if (conts != null)
                {
                    cbxContainer.Items.Clear();
                    foreach (var cont in conts)
                    {
                        cbxContainer.Items.Add(cont);
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show($"There was an error retrieving the container list: {x.Message}");
            }
        }

        #endregion

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private class RequestParams
        {
            public string ContainerName;
            public string UserName;
            public string ApiKey;
            public string[] KeyValue;
        }
    }
}
