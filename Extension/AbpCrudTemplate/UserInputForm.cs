using System;
using System.Windows.Forms;

namespace AbpCrudTemplate
{
    public partial class UserInputForm : Form
    {
        public UserInputForm()
        {
            InitializeComponent();
        }
        public string PluralEntityName { get; set; }
        public string MicroServiceName { get; set; }
        public bool IsMultiTenant { get; set; } = false;
        public string Properties { get; set; }
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.PluralEntityName = txtPluralEntityName.Text;
            this.MicroServiceName = txtMicroServiceName.Text;
            this.Properties = txtProperties.Text;
            this.Close();
        }
    }
}
