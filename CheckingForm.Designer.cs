﻿namespace languagetool_msword10_addin
{
    partial class CheckingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckingForm));
            this.suggestionsBox = new System.Windows.Forms.ListBox();
            this.contextTextBox = new System.Windows.Forms.RichTextBox();
            this.changeSuggestion = new System.Windows.Forms.Button();
            this.ignoreSuggestion = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.messageBox = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // suggestionsBox
            // 
            this.suggestionsBox.FormattingEnabled = true;
            this.suggestionsBox.Location = new System.Drawing.Point(23, 180);
            this.suggestionsBox.Name = "suggestionsBox";
            this.suggestionsBox.Size = new System.Drawing.Size(245, 121);
            this.suggestionsBox.TabIndex = 0;
            // 
            // contextTextBox
            // 
            this.contextTextBox.Location = new System.Drawing.Point(23, 12);
            this.contextTextBox.Name = "contextTextBox";
            this.contextTextBox.Size = new System.Drawing.Size(245, 94);
            this.contextTextBox.TabIndex = 1;
            this.contextTextBox.Text = "";
            this.contextTextBox.TextChanged += new System.EventHandler(this.textUpdated);
            // 
            // changeSuggestion
            // 
            this.changeSuggestion.Enabled = false;
            this.changeSuggestion.Location = new System.Drawing.Point(280, 180);
            this.changeSuggestion.Name = "changeSuggestion";
            this.changeSuggestion.Size = new System.Drawing.Size(100, 23);
            this.changeSuggestion.TabIndex = 2;
            this.changeSuggestion.Text = "Reemplaça";
            this.changeSuggestion.UseVisualStyleBackColor = true;
            this.changeSuggestion.Click += new System.EventHandler(this.changeSuggestion_Click);
            // 
            // ignoreSuggestion
            // 
            this.ignoreSuggestion.Location = new System.Drawing.Point(280, 210);
            this.ignoreSuggestion.Name = "ignoreSuggestion";
            this.ignoreSuggestion.Size = new System.Drawing.Size(100, 23);
            this.ignoreSuggestion.TabIndex = 3;
            this.ignoreSuggestion.Text = "Ignora";
            this.ignoreSuggestion.UseVisualStyleBackColor = true;
            this.ignoreSuggestion.Click += new System.EventHandler(this.ignoreSuggestion_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 162);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Suggeriments";
            // 
            // messageBox
            // 
            this.messageBox.BackColor = System.Drawing.SystemColors.Control;
            this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.messageBox.Location = new System.Drawing.Point(23, 112);
            this.messageBox.Name = "messageBox";
            this.messageBox.Size = new System.Drawing.Size(245, 47);
            this.messageBox.TabIndex = 5;
            this.messageBox.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(280, 270);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Cancel·la";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.cancel_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(280, 240);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Ignora sempre";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ignoreAlwaysSuggestion_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(277, 112);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(78, 13);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Més informació";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // CheckingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 310);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ignoreSuggestion);
            this.Controls.Add(this.changeSuggestion);
            this.Controls.Add(this.contextTextBox);
            this.Controls.Add(this.suggestionsBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CheckingForm";
            this.Text = "Revisió ortogràfica i gramatical";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ListBox suggestionsBox;
        public System.Windows.Forms.RichTextBox contextTextBox;
        public System.Windows.Forms.Button changeSuggestion;
        private System.Windows.Forms.Button ignoreSuggestion;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox messageBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.LinkLabel linkLabel1;
    }
}