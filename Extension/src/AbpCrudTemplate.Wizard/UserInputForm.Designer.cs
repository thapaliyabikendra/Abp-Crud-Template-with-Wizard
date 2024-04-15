namespace AbpCrudTemplate.Wizard
{
    partial class UserInputForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPluralEntityName = new System.Windows.Forms.TextBox();
            this.txtProperties = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbAddMigration = new System.Windows.Forms.CheckBox();
            this.cbUpdateDatabase = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(70, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Plural Entity Name:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(70, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Properties:";
            // 
            // txtPluralEntityName
            // 
            this.txtPluralEntityName.Location = new System.Drawing.Point(240, 44);
            this.txtPluralEntityName.Name = "txtPluralEntityName";
            this.txtPluralEntityName.Size = new System.Drawing.Size(366, 22);
            this.txtPluralEntityName.TabIndex = 5;
            // 
            // txtProperties
            // 
            this.txtProperties.Location = new System.Drawing.Point(240, 93);
            this.txtProperties.Name = "txtProperties";
            this.txtProperties.Size = new System.Drawing.Size(366, 22);
            this.txtProperties.TabIndex = 6;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(311, 252);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(524, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cbAddMigration
            // 
            this.cbAddMigration.AutoSize = true;
            this.cbAddMigration.Location = new System.Drawing.Point(240, 150);
            this.cbAddMigration.Name = "cbAddMigration";
            this.cbAddMigration.Size = new System.Drawing.Size(112, 20);
            this.cbAddMigration.TabIndex = 10;
            this.cbAddMigration.Text = "Add Migration";
            this.cbAddMigration.UseVisualStyleBackColor = true;
            // 
            // cbUpdateDatabase
            // 
            this.cbUpdateDatabase.AutoSize = true;
            this.cbUpdateDatabase.Location = new System.Drawing.Point(240, 176);
            this.cbUpdateDatabase.Name = "cbUpdateDatabase";
            this.cbUpdateDatabase.Size = new System.Drawing.Size(137, 20);
            this.cbUpdateDatabase.TabIndex = 11;
            this.cbUpdateDatabase.Text = "Update Database";
            this.cbUpdateDatabase.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(240, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 15);
            this.label2.TabIndex = 12;
            this.label2.Text = "ex. DisplayName:string:R";
            // 
            // UserInputForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(735, 321);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbUpdateDatabase);
            this.Controls.Add(this.cbAddMigration);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtProperties);
            this.Controls.Add(this.txtPluralEntityName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Name = "UserInputForm";
            this.Text = "UserInputForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPluralEntityName;
        private System.Windows.Forms.TextBox txtProperties;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbAddMigration;
        private System.Windows.Forms.CheckBox cbUpdateDatabase;
        private System.Windows.Forms.Label label2;
    }
}