﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WowRealmStatus
{
    public partial class MainForm : Form
    {
        private ListViewColumnSorter columnSorter;
        private string m_region;

        public MainForm()
        {
            InitializeComponent();

            // Create an instance of a ListView column sorter and assign it to the ListView control.
            columnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = columnSorter;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_region = "eu";

            UpdateRealmStatus();
        }

        private void UpdateRealmStatus()
        {
            Task<RealmStatus>.Factory.StartNew(() => RealmStatus.Get(m_region)).ContinueWith(task => FillListView(task.Result));
        }

        delegate void AddListViewItem(RealmStatus r);

        private void FillListView(RealmStatus status)
        {
            if (status == null)
            {
                MessageBox.Show(String.Format("Failed to get realm status for region {0}", m_region));
                return;
            }

            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new AddListViewItem(FillListView), status);
            }
            else
            {
                listView1.Items.Clear();

                listView1.BeginUpdate();
                foreach (var realm in status.realms)
                    listView1.Items.Add(new ListViewItem(realm.ToStringArray()));
                listView1.EndUpdate();
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            columnSorter.SortColumn = e.Column;
            listView1.Sorting = listView1.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            columnSorter.Order = listView1.Sorting;
            listView1.Sort();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRealmStatus();
        }

        private void regionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem region in regionToolStripMenuItem.DropDownItems)
            {
                if (region != sender)
                    region.Checked = false;
                else
                    m_region = (string)region.Tag;
            }

            UpdateRealmStatus();
        }
    }
}
