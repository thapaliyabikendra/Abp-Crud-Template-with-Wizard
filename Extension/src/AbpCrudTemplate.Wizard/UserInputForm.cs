using System;
using System.Windows.Forms;

namespace AbpCrudTemplate.Wizard
{
    public partial class UserInputForm : Form
    {
        public UserInputForm()
        {
            InitializeComponent();
        }
        public string PluralEntityName { get; set; }
        public string Properties { get; set; }
        public bool AddMigration { get; set; }
        public bool UpdateDatabase { get; set; }
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.PluralEntityName = txtPluralEntityName.Text;
            this.Properties = txtProperties.Text;
            this.AddMigration = cbAddMigration.Checked;
            this.UpdateDatabase = cbUpdateDatabase.Checked;
            this.Close();
        }
    }
}
