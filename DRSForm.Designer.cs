
namespace DRS
{
    partial class DRSForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DRSForm));
            this.Btn_STSP = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Txt_Message = new System.Windows.Forms.TextBox();
            this.chk_Info = new System.Windows.Forms.CheckBox();
            this.chk_TxtReturn = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.PortNum = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNum)).BeginInit();
            this.SuspendLayout();
            // 
            // Btn_STSP
            // 
            this.Btn_STSP.BackColor = System.Drawing.Color.White;
            this.Btn_STSP.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Btn_STSP.Location = new System.Drawing.Point(9, 3);
            this.Btn_STSP.Margin = new System.Windows.Forms.Padding(0);
            this.Btn_STSP.Name = "Btn_STSP";
            this.Btn_STSP.Size = new System.Drawing.Size(62, 29);
            this.Btn_STSP.TabIndex = 0;
            this.Btn_STSP.Text = "Start";
            this.Btn_STSP.UseVisualStyleBackColor = false;
            this.Btn_STSP.Click += new System.EventHandler(this.Btn_STSP_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(80, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            // 
            // Txt_Message
            // 
            this.Txt_Message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Txt_Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Txt_Message.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Txt_Message.Location = new System.Drawing.Point(0, 0);
            this.Txt_Message.Margin = new System.Windows.Forms.Padding(0);
            this.Txt_Message.Multiline = true;
            this.Txt_Message.Name = "Txt_Message";
            this.Txt_Message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Txt_Message.Size = new System.Drawing.Size(700, 205);
            this.Txt_Message.TabIndex = 4;
            this.Txt_Message.WordWrap = false;
            // 
            // chk_Info
            // 
            this.chk_Info.AutoSize = true;
            this.chk_Info.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chk_Info.Location = new System.Drawing.Point(299, 8);
            this.chk_Info.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chk_Info.Name = "chk_Info";
            this.chk_Info.Size = new System.Drawing.Size(105, 21);
            this.chk_Info.TabIndex = 3;
            this.chk_Info.Text = "詳細情報表示";
            this.chk_Info.UseVisualStyleBackColor = true;
            // 
            // chk_TxtReturn
            // 
            this.chk_TxtReturn.AutoSize = true;
            this.chk_TxtReturn.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chk_TxtReturn.Location = new System.Drawing.Point(223, 8);
            this.chk_TxtReturn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chk_TxtReturn.Name = "chk_TxtReturn";
            this.chk_TxtReturn.Size = new System.Drawing.Size(70, 21);
            this.chk_TxtReturn.TabIndex = 2;
            this.chk_TxtReturn.Text = "折り返し";
            this.chk_TxtReturn.UseVisualStyleBackColor = true;
            this.chk_TxtReturn.CheckedChanged += new System.EventHandler(this.Chk_TxtReturn_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.Color.LightGray;
            this.splitContainer1.Location = new System.Drawing.Point(0, 34);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Txt_Message);
            this.splitContainer1.Size = new System.Drawing.Size(700, 411);
            this.splitContainer1.SplitterDistance = 203;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 5;
            // 
            // dataGridView1
            // 
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(700, 203);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
            // 
            // PortNum
            // 
            this.PortNum.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PortNum.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.PortNum.Location = new System.Drawing.Point(123, 10);
            this.PortNum.Margin = new System.Windows.Forms.Padding(0);
            this.PortNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortNum.Name = "PortNum";
            this.PortNum.Size = new System.Drawing.Size(70, 20);
            this.PortNum.TabIndex = 1;
            this.PortNum.Value = new decimal(new int[] {
            8085,
            0,
            0,
            0});
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(700, 3);
            this.panel1.TabIndex = 6;
            // 
            // DRSForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(700, 446);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.PortNum);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.chk_TxtReturn);
            this.Controls.Add(this.chk_Info);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Btn_STSP);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "DRSForm";
            this.Text = "ダミー応答サーバー";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DmSVForm_FormClosing);
            this.Load += new System.EventHandler(this.DmSVForm_Load);
            this.Resize += new System.EventHandler(this.DmSVForm_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Btn_STSP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Txt_Message;
        private System.Windows.Forms.CheckBox chk_Info;
        private System.Windows.Forms.CheckBox chk_TxtReturn;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.NumericUpDown PortNum;
        private System.Windows.Forms.Panel panel1;
    }
}

