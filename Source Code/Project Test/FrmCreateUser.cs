using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Project_Test
{
    public partial class FrmCreateUser : Form
    {
        SqlDataAdapter daUsers;
        DataSet dsRecipeFilter = new DataSet();
        SqlCommandBuilder cmdBUsers;
        DataRow  drUsers;
        String connStr, sqlUsers;
        SqlDataReader dr;
        int id = 0;
        
        //Loads in relevant information from the database for the create users form
        public FrmCreateUser()
        {
            //This must be changed to the server the database is running on as shown in the report submitted with this application
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            connStr = @"Data Source=DESKTOP-D93M828\SQLEXPRESS;Initial Catalog=RecipeFilter;Integrated Security=True";
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            InitializeComponent();
            loadUsers();
            getNumber();
            errp.Clear();
        }

        //Gets the id number for the new user being added
        private void getNumber()
        {
            int i = 0;
            foreach (DataRow dr in dsRecipeFilter.Tables["Users"].Rows)
            {
                if (i < Convert.ToInt32(dr["userId"].ToString()))
                {
                    i = Convert.ToInt32(dr["userId"].ToString());
                }
            }
            id = i + 1;
            lblId.Text = "ID: " + (id);
        }

        //Closes the add user form if yes is selected
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult yesNo = MessageBox.Show("Are you sure you would like to\ncancel adding a new user?", "Confirm Cancel", MessageBoxButtons.YesNo);
            if (yesNo == DialogResult.Yes)
                this.Close();
        }

        //Adds the new user to the database
        private void btnAddUser_Click(object sender, EventArgs e)
        {
            errp.Clear();
            //If there is no validation problems, then the user is added aslong as yes is selected. The form then closes
            if(validateGoals())
            {
                DialogResult yesNo = MessageBox.Show("Are you sure you would like to add user: " + lblId.Text.ToString() + " to the database?", "Confirm Addition", MessageBoxButtons.YesNo);
                if (yesNo == DialogResult.Yes)
                {
                    drUsers = dsRecipeFilter.Tables["Users"].NewRow();
                    drUsers["userId"] = id;
                    drUsers["currentWeight"] = txtCurWeight.Text.ToString().Trim();
                    drUsers["goalWeight"] = txtGoalWeight.Text.ToString().Trim();
                    dsRecipeFilter.Tables["Users"].Rows.Add(drUsers);
                    daUsers.Update(dsRecipeFilter, "Users");
                    MessageBox.Show("User: " + lblId.Text.ToString() + " has been added successfully.");
                    this.Close();
                }
            }
        }

        //Validation for the two goal inputs
        private bool validateGoals()
        {
            bool ok = true;
            String currentWeight = txtCurWeight.Text.ToString().Trim();
            String goalWeight = txtGoalWeight.Text.ToString().Trim();

            //Ensures that if something is entered, it is only numeric
            if (!Regex.IsMatch(currentWeight, @"^\d+$") && currentWeight != "")
            {
                errp.SetError(txtCurWeight, "Only numeric values can be entered.");
                ok = false;
            }
            //Ensures that if nothing is entered, the system will throw an error
            else if (currentWeight.Length == 0)
            {
                errp.SetError(txtCurWeight, "You must enter your current weight to create a user.");
                ok = false;
            }

            //Ensures that if something is entered, it is only numeric
            if (!Regex.IsMatch(goalWeight, @"^\d+$") && goalWeight != "")
            {
                errp.SetError(txtGoalWeight, "Only numeric values can be entered.");
                ok = false;
            }
            //Ensures that if nothing is entered, the system will throw an error
            else if (goalWeight.Length == 0)
            {
                errp.SetError(txtGoalWeight, "You must enter your current weight to create a user.");
                ok = false;
            }

            return ok;
        }

        //Loads users from the database
        private void loadUsers()
        {
            sqlUsers = @"select * from Users";
            daUsers = new SqlDataAdapter(sqlUsers, connStr);
            cmdBUsers = new SqlCommandBuilder(daUsers);
            daUsers.FillSchema(dsRecipeFilter, SchemaType.Source, "Users");
            daUsers.Fill(dsRecipeFilter, "Users");
        }

    }
}
