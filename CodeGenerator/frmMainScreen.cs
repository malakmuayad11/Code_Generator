using CodeGenerator_Business;
using Logger;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeGenerator
{
    public partial class frmMainScreen : Form
    {
        public frmMainScreen() => InitializeComponent();

        /// <summary>
        /// Sets and updates the connection string for the specified database.
        /// </summary>
        /// <remarks>This method modifies the connection string settings in the application's
        /// configuration file. If a connection string with the name "DBConnection" already exists, it will be updated;
        /// otherwise, a new connection string will be added.</remarks>
        /// <param name="DatabaseName">The name of the database to connect to. Cannot be null or empty.</param>
        /// <param name="UserID">The user ID for database authentication. Cannot be null or empty.</param>
        /// <param name="Password">The password for database authentication. Cannot be null or empty.</param>
        private void _SetConnectionString(string DatabaseName, string UserID, string Password)
        {
            // Open the configuration file for the current executable
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Define connection string details
            string connectionName = "DBConnection";
            string newConnectionString =
                $"Initial Catalog={DatabaseName};Server=.;Database={DatabaseName};User ID={UserID};Password={Password}";
            string providerName = "System.Data.SqlClient";

            // Check if it already exists
            ConnectionStringSettings existingConnection = config.ConnectionStrings.ConnectionStrings[connectionName];

            if (existingConnection == null)
            {
                // Add new connection string
                config.ConnectionStrings.ConnectionStrings.Add(
                    new ConnectionStringSettings(connectionName, newConnectionString, providerName));
            }
            else
            {
                // Update existing connection string
                existingConnection.ConnectionString = newConnectionString;
                existingConnection.ProviderName = providerName;
            }

            // Save and refresh the section
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        /// <summary>
        /// Generates C# CRUD code for all tables provided from the database.
        /// <remarks>This method generates C# for all tables from the provided database. If the database does 
        /// not exists, or the userID or the password are wrong, the code won't be generated.</remarks>
        /// </summary>
        /// <returns>Whether the CRUD code generated successfully or not.</returns>
        private async Task<bool> _GenerateCode()
        {
            string ProjectName = txtProjectName.Text.Trim();
            string DatabaseName = txtDatabaseName.Text.Trim();

            _SetConnectionString(ProjectName, txtUserID.Text.Trim(), txtPassword.Text.Trim());

            DataTable tableNames = await clsTable.GetTablesForDatabase(DatabaseName);

            if (tableNames.Rows.Count == 0)
            {
                MessageBox.Show("No tables found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            foreach (DataRow row in tableNames.Rows)
            {
                string tableName = row["TableName"].ToString();
                await new clsTable(DatabaseName, tableName, new clsFileGenerator(tableName, ProjectName),
                    new clsGeneratorData(tableName, ProjectName), await clsGeneratorBusiness.CreateAsync(tableName, ProjectName)).GenerateCRUDCode();
            }

            return true;
        }

        /// <summary>
        /// Simulates the generating process of C# code.
        /// </summary>
        /// <returns>Whether the simulation process successed or not.</returns>
        private async Task<bool> _Generate()
        {
            bool result = false;

            lblScreenTitle.Text = "Generating....";
            lblScreenTitle.ForeColor = Color.Red;

            await Task.Delay(200);
            if (await _GenerateCode())
                result = true;

            Task.WaitAll();

            lblScreenTitle.Text = "Code Generator";
            lblScreenTitle.ForeColor = Color.Black;
            return result;
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if(!this.ValidateChildren())
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!await _Generate())
                return;

            MessageBox.Show($"Code generation completed successfully at: C:/{txtProjectName.Text.Trim()}.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information); // The default path for the generated code is C:/
        }

        private void _ValidateRequiredTextBox(object sender, System.ComponentModel.CancelEventArgs e) =>
            clsValidation.ValidateRequiredTextBox((TextBox)sender, errorProvider1, e);

        /// <summary>
        /// Gets the stored UserID and password from windows registry of this application.
        /// </summary>
        /// <returns>UserID and Password from registry</returns>
        private static async Task<(string UserID, string Password)> _GetUserIDAndPassword()
        {
            string UserID = string.Empty;
            string Password = string.Empty;
            try
            {
                UserID = await clsRegistry.ReadFromRegistry("UserID");
                Password = await clsRegistry.ReadFromRegistry("Password");
            }
            catch (Exception ex)
            {
                await clsLogger.Log(ex.Message, EventLogEntryType.Error);
            }
            return (UserID, Password);
        }

        /// <summary>
        /// Fills the UserID and Passowrd fields with appropriate values from registry.
        /// </summary>
        /// <returns>Asynchrounus task that fills UserID and Password fiels.</returns>
        private async Task _FillLoginFields()
        {
            (string UserID, string Password) = await _GetUserIDAndPassword();

            txtUserID.Text = string.IsNullOrEmpty(UserID) ? string.Empty : UserID;

            if (!string.IsNullOrEmpty(Password))
                txtPassword.Text = clsAES.Decrypt_AES(Password, clsAES.Key);
        }

        private async void chkRememberMe_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRememberMe.Checked)
            {
                if (string.IsNullOrEmpty(txtUserID.Text)
                    || string.IsNullOrEmpty(txtPassword.Text))
                    return;

                // Store values in windows registry if chkRembmerMe is checked, and values are not empty.
                await clsRegistry.WriteInRegistry("UserID", txtUserID.Text.Trim());
                await clsRegistry.WriteInRegistry("Password", clsAES.Encrypt_AES(txtPassword.Text.Trim(),
                    clsAES.Key));
            }
            else
            {
                // If chkRememberMe is not checked, store empty values in the registry.
                await clsRegistry.WriteInRegistry("UserID", string.Empty);
                await clsRegistry.WriteInRegistry("Password", string.Empty);
            }
        }

        private async void frmMainScreen_Load(object sender, EventArgs e) => await _FillLoginFields();
    }
}