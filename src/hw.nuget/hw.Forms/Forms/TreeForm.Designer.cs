using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Forms
{
    sealed partial class TreeForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(554, 565);
            this.treeView1.TabIndex = 0;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "none");
            this.imageList1.Images.SetKeyName(1, "none open");
            this.imageList1.Images.SetKeyName(2, "Syntax");
            this.imageList1.Images.SetKeyName(3, "Symbol");
            this.imageList1.Images.SetKeyName(4, "Context");
            this.imageList1.Images.SetKeyName(5, "Code");
            this.imageList1.Images.SetKeyName(6, "Pending");
            this.imageList1.Images.SetKeyName(7, "Cache");
            this.imageList1.Images.SetKeyName(8, "Pending Cache");
            this.imageList1.Images.SetKeyName(9, "Ok");
            this.imageList1.Images.SetKeyName(10, "Syntax Old");
            this.imageList1.Images.SetKeyName(11, "Dictionary");
            this.imageList1.Images.SetKeyName(12, "List");
            this.imageList1.Images.SetKeyName(13, "String");
            this.imageList1.Images.SetKeyName(14, "Question");
            this.imageList1.Images.SetKeyName(15, "Number");
            this.imageList1.Images.SetKeyName(16, "Type");
            this.imageList1.Images.SetKeyName(17, "ListItem");
            this.imageList1.Images.SetKeyName(18, "Key");
            this.imageList1.Images.SetKeyName(19, "Bool");
            this.imageList1.Images.SetKeyName(20, "Size");
            this.imageList1.Images.SetKeyName(21, "CodeError");
            // 
            // TreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 565);
            this.Controls.Add(this.treeView1);
            this.Name = "TreeForm";
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
    }
}