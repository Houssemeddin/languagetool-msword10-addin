﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace languagetool_msword10_addin
{
    public partial class TaskPaneControl : UserControl
    {
        public TaskPaneControl()
        {
            InitializeComponent();
        }

        private void serverComboBoxChanged(object sender, EventArgs e)
        {
            ThisAddIn.setLTServer(this.comboBox1.Text.ToString());
        }

    }
}
