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
    public partial class FrmAddIngredient : Form
    {
        SqlDataAdapter daIngredients;
        DataSet dsRecipeFilter = new DataSet();
        SqlCommandBuilder cmdBIngredients;
        DataRow drIngredients;
        String connStr, sqlIngredients;
        SqlDataReader dr;
        int id = 0;
        public FrmAddIngredient()
        {
            InitializeComponent();
            //This must be changed to the server the database is running on as shown in the report submitted with this application
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            connStr = @"Data Source=DESKTOP-D93M828\SQLEXPRESS;Initial Catalog=RecipeFilter;Integrated Security=True";
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            loadIngredients();
            getNumber();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool ok = false;
            String ingredientName = Regex.Replace(txtAdd.Text.Trim(), @"\s+", " ");

            if (ingredientName.Length <= 3 || ingredientName.Trim().Length > 50)
                MessageBox.Show("Ingredient names must be \nbetween 3 and 50 characters.");
            else if (Regex.IsMatch(ingredientName, @"^[a-zA-Z\s]+$"))
                ok = true;
            else
                MessageBox.Show("Please enter only letters for the name.");

            if(ok)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you would like to add '" + ingredientName + "' to the database?", "Confirm Addition", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    drIngredients = dsRecipeFilter.Tables["Ingredients"].NewRow();
                    drIngredients["ingredientId"] = id;
                    drIngredients["ingredientName"] = ingredientName;
                    dsRecipeFilter.Tables["Ingredients"].Rows.Add(drIngredients);
                    daIngredients.Update(dsRecipeFilter, "Ingredients");
                    MessageBox.Show(ingredientName + " has been added successfully.");
                    resetAdd();
                }
                    
            }
        }
    
        private void loadIngredients()
        {
            sqlIngredients = @"select * from Ingredients";
            daIngredients = new SqlDataAdapter(sqlIngredients, connStr);
            cmdBIngredients = new SqlCommandBuilder(daIngredients);
            daIngredients.FillSchema(dsRecipeFilter, SchemaType.Source, "Ingredients");

            daIngredients.Fill(dsRecipeFilter, "Ingredients");
        }

        private void getNumber()
        {
            int i = 0;
            foreach (DataRow dr in dsRecipeFilter.Tables["Ingredients"].Rows)
            {
                if (i < Convert.ToInt32(dr["ingredientId"].ToString()))
                {
                    i = Convert.ToInt32(dr["ingredientId"].ToString());
                }
            }
            id = i + 1;
            lbliD.Text = "ID: " + (id);
        }
    
        private void resetAdd()
        {
            txtAdd.Text = "";
            getNumber();
        }
    }
}
