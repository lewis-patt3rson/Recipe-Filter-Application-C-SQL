using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Project_Test
{
    public partial class Form1 : Form
    {
        SqlDataAdapter daIngredients, daSearch, daRecipes, daRecipeIngredients, daCuisines, daRecipeCuisines, daUsers, daUserRecipes;
        DataSet dsRecipeFilter = new DataSet();
        SqlCommandBuilder cmdBIngredients, cmdBSearch, cmdBRecipes, cmdBRecipeIngredients, cmdBCuisines, cmdBRecipeCuisines, cmdBUsers, cmdBUserRecipes;
        DataRow drIngredients, drRecipes, drRecipeIngredients, drCuisines, drRecipeCuisines, drUsers, drUserRecipes;
        String connStr, sqlIngredients, connSearch ,sqlRecipes, sqlRecipeIngredients, sqlCuisines, sqlRecipeCuisines, sqlUsers, sqlUserRecipes;
        SqlDataReader dr;
        int errorCount = 0, userId =0;

        public Form1()
        {
            InitializeComponent();

        }      

        //Updates ingredients based on what is typed in textbox
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            updateSearch();
        }

        //Checks login validation after the user presses log in
        private void btnLogIn_Click(object sender, EventArgs e)
        {
            errp.Clear();
            logInValidation();
        }

        //Removes the exclusion tag on excluded cuisines, making them no longer excluded
        private void btnRmvExc_Click(object sender, EventArgs e)
        {
            //Checks if the selected row is an excluded cuisine, if so the color is reset and the '-' tag is removed
            if(dgvCuisines.CurrentRow.Cells[1].Value.ToString().Substring(0,1) == "-")
            {
                dgvCuisines.CurrentRow.DefaultCellStyle.BackColor = Color.White;
                dgvCuisines.CurrentRow.Cells[1].Value = (dgvCuisines.CurrentRow.Cells[1].Value.ToString().Substring(1));
            }
        }

        //Depending on which tab is selected, it is reset to its default state (Refreshed)
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(tabControl1.SelectedIndex)
            {
                case 0: resetFilterTab(); break;
                case 1: resetDetailsTab(); break;
                case 2: resetUserTab(); break;
                default: break;
            }
        }

        //Removes the exclusion of all cuisines, making them all no longer excluded
        private void refreshCuisines()
        {
            //Checks if the row is excluded and if so, resets it to no longer excluded and removes the '-' tag
            foreach(DataGridViewRow row in dgvCuisines.Rows)
                if (row.Cells[1].Value.ToString().Substring(0, 1) == "-")
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.Cells[1].Value = (row.Cells[1].Value.ToString().Substring(1));
                }
        }

        //Loads the add ingredient form
        private void button1_Click(object sender, EventArgs e)
        {
            FrmAddIngredient addIngForm = new FrmAddIngredient();
            addIngForm.FormClosing += new FormClosingEventHandler(this.FrmAddClose);
            addIngForm.ShowDialog(this);
        }

        //Tags cuisines as excluded
        private void btnAddExc_Click(object sender, EventArgs e)
        {
            dgvCuisines.CurrentRow.DefaultCellStyle.BackColor = Color.FromArgb(255, 204, 203);
            dgvCuisines.CurrentRow.Cells[1].Value = ("-" + dgvCuisines.CurrentRow.Cells[1].Value.ToString());               
        }

        //Loads the create user form
        private void btnCreateUsers_Click(object sender, EventArgs e)
        {
            FrmCreateUser frmNewU = new FrmCreateUser();
            frmNewU.ShowDialog(this);
        }

        //Resets the ingredients when the add ingredients form closes to refresh the dgv and show the new ingredients
        private void FrmAddClose(object sender, FormClosingEventArgs e)
        {
            resetFilterTab();
        }

        //Resets the logged in user id and refreshes the user tab
        private void btnLogOut_Click(object sender, EventArgs e)
        {
            userId = 0;
            resetUserTab();
        }

        //Creates a connection to the SSMS database 'RecipeFilter' allowing it to be utilised in the windows form
        private void Form1_Load(object sender, EventArgs e)
        {
            //The connection string to the local server running the database. 
            //This must be changed to the server the database is running on as shown in the report submitted with this application
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            connStr = @"Data Source=DESKTOP-D93M828\SQLEXPRESS;Initial Catalog=RecipeFilter;Integrated Security=True";
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Fills DGVS
            loadIngredients();
            loadRecipes();
            loadCuisines();
            loadRecipeCuisines();
            loadUsers();
            loadUserCuisines();
            loadUserRecipes();
            fillCombo();
        }

        //Resets the filter tab
        private void btnRemoveFilters_Click(object sender, EventArgs e)
        {
            resetFilterTab();
        }

        //Loads the cuisines in the user tab, to be selected as a preferred cuisine
        private void btnRemovePref_Click(object sender, EventArgs e)
        {
            loadUserCuisines();
        }

        //Highlights the selected preference in the user tab
        private void btnSetPrefference_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvUserCuisines.Rows)
                row.DefaultCellStyle.BackColor = Color.White;
            dgvUserCuisines.CurrentRow.DefaultCellStyle.BackColor = Color.Green;
        }

        //Enables the edit panel 
        private void btnEdit_Click(object sender, EventArgs e)
        {
            foreach (Control c in pnlUser.Controls)
                c.Enabled = true;
        }

        //Checks if text entered in the textbox is onlu numeric and that something is actually entered. 
        private void editValidation(TextBox txt)
        {
            String text = txt.Text.ToString().Trim(); 
            if (!Regex.IsMatch(text, @"^\d+$") && text != "")
            {
                errp.SetError(txt, "Only numeric values can be entered.");
                errorCount++;
            }
            //Ensures that if nothing is entered, the system will throw an error
            else if (text.Length == 0)
            {
                errp.SetError(txt, "Only numeric values can be entered.");
                errorCount++;
            }
        }

        //Updates the text above the log panel of what recipe is selected (If none, then the text is set to empty
        private void dgvRecDetails_SelectionChanged(object sender, EventArgs e)
        {
            if(dgvRecDetails.Rows.Count > 0)
            {
                lblRecId.Text = dgvRecDetails.CurrentRow.Cells[0].Value.ToString();
                lblRecName.Text = dgvRecDetails.CurrentRow.Cells[1].Value.ToString();
            }
            else
            {
                lblRecId.Text = "";
                lblRecName.Text = "";
            }
        }

        //Edits an existing user
        private void button5_Click(object sender, EventArgs e)
        {
            //Clears errors and checks each textbox individually, if there is an error then errorCount++
            errp.Clear();
            errorCount = 0;
            editValidation(txtCurWeight);
            editValidation(txtGoalWeight);
            editValidation(txtDailyGoalCal);
            editValidation(txtDailyGoalCarbs);
            editValidation(txtDailyGoalFat);
            editValidation(txtDailyGoalPro);
             
            if(errorCount == 0)
            {
                //If there are no errors, the user is asked if they are sure they would like to save this and if so, the new details are stored for the existing user
                DialogResult yesNo = MessageBox.Show("Are you sure you would like to edit user: "+lblId.Text+"?", "Confirm Edit", MessageBoxButtons.YesNo);
                if (yesNo == DialogResult.Yes)
                {
                    drUsers.BeginEdit();
                    drUsers["userId"] = Convert.ToInt32(lblId.Text.ToString());
                    drUsers["currentWeight"] = Convert.ToInt32(txtCurWeight.Text.ToString());
                    drUsers["goalWeight"] = Convert.ToInt32(txtGoalWeight.Text.ToString());
                    drUsers["calorieGoal"] = Convert.ToInt32(txtDailyGoalCal.Text.ToString());
                    drUsers["proteinGoal"] = Convert.ToInt32(txtDailyGoalPro.Text.ToString());
                    drUsers["carbGoal"] = Convert.ToInt32(txtDailyGoalCarbs.Text.ToString());
                    drUsers["fatGoal"] = Convert.ToInt32(txtDailyGoalFat.Text.ToString());
                    //Checks which cuisine is highlighted to set the preferred cuisine
                    foreach (DataGridViewRow row in dgvUserCuisines.Rows)
                        if (row.DefaultCellStyle.BackColor == Color.Green)
                            drUsers["preferredCuisine"] = Convert.ToInt32(row.Cells[0].Value.ToString());

                    drUsers.EndEdit();
                    daUsers.Update(dsRecipeFilter, "Users");

                    MessageBox.Show("User's details updated.");

                    foreach (Control c in pnlUser.Controls)
                        c.Enabled = false;

                    logIn();

                }
            }
        }

        //Logs recipes for users meals per day
        private void btnLogFood_Click(object sender, EventArgs e)
        {
            
            if(cmbMealTime.SelectedIndex == -1)
            {
                errp.SetError(cmbMealTime, "You must choose what type of meal you had this recipe for.");
            }
            else if(dgvRecDetails.CurrentRow == null)
            {
                errp.SetError(dgvRecDetails, "You must choose a recipe in order to log a meal.");
            }
            else
            {
                
                String today = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
                bool exists = false;
                //If the user has already logged a meal for that time period of that day, they will be asked to confirm overwriting it.
                foreach (DataRow dr in dsRecipeFilter.Tables["userRecipes"].Rows)
                {
                    if (dr["dateLogged"].ToString().Substring(0, 10) == today && Convert.ToInt32(dr["userId"].ToString()) == userId && dr["mealTime"].ToString() == cmbMealTime.Text.ToString())
                    {
                        exists = true;
                        DialogResult dialogResult = MessageBox.Show("You have already logged a meal for " + cmbMealTime.Text.ToString() + " Today.\nWould you like to overwrite it ? ", "Confirm Overwrite", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            logMeal();
                        }
                    }
                }
                //If the user has not logged a meal for that time period of that day, then the meal is logged.
                if(!exists)
                {
                    DialogResult dialogResult = MessageBox.Show("Are you sure you would like to log this\nrecipe for " + cmbMealTime.Text.ToString()+"?", "Confirm Log", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        logMeal();
                    }
                }
            }
        }

        //Logs user's meal for a time of day to the database
        private void logMeal()
        {
            drUserRecipes = dsRecipeFilter.Tables["UserRecipes"].NewRow();
            drUserRecipes["userId"] = userId;
            drUserRecipes["recipeId"] = dgvRecDetails.CurrentRow.Cells[0].Value.ToString();
            drUserRecipes["dateLogged"] = String.Format("{0:MM-dd-yyyy}", DateTime.Now);
            drUserRecipes["mealTime"] = cmbMealTime.Text.ToString();
            dsRecipeFilter.Tables["UserRecipes"].Rows.Add(drUserRecipes);
            daUserRecipes.Update(dsRecipeFilter, "UserRecipes");
            MessageBox.Show("Meal Logged Successfully.");
            cmbMealTime.SelectedIndex = -1;
        }

        //When typing in the search bar of the details tab, the dgv automatically updates.
        private void txtRecSearch_TextChanged(object sender, EventArgs e)
        {
            updateRecSearch();
        }

        //Resets the filter tab to its original state
        private void resetFilterTab()
        {
            dgvSearch.Rows.Clear();
            refreshCuisines();
            loadRecipes();
            loadIngredients();
            clearMinMax();
            errp.Clear();
            errorCount = 0;
            txtSearch.Text = "";
            highlightPrefferedCuisines();
        }

        //Loads the recipe tab when details is clicked
        private void button2_Click(object sender, EventArgs e)
        {
            if(dgvRecipes.CurrentRow != null)            
                tabControl1.SelectedIndex = 1;
        }

        //Resets the details tab to its original state
        private void resetDetailsTab()
        {
            errp.Clear();
            fillRecDetails();
            cmbMealTime.SelectedIndex = -1;
            txtRecSearch.Text = "";
            if (userId != 0)
                foreach (Control c in pnlLog.Controls)
                    c.Enabled = true;
            else
                foreach (Control c in pnlLog.Controls)
                    c.Enabled = false;
        }

        //Loads items into combo box
        private void fillCombo()
        {
            cmbMealTime.Items.Add("Breakfast");
            cmbMealTime.Items.Add("Lunch");
            cmbMealTime.Items.Add("Dinner");
            cmbMealTime.Items.Add("Other");
        }

        //Resets the user tab to its original state
        private void resetUserTab()
        {
            loadUserCuisines();
            errp.Clear();
            txtLogIn.Text = "";
            if(userId != 0)
            {
                logIn();
            }
            else
            {
                foreach (Control c in pnlUser.Controls)
                    c.Enabled = false;
                emptyUserDetails();
            }
            lblDate.Text = DateTime.UtcNow.ToString("dd-MM-yyyy");
            
        }

        //Applys filters onto the recipes
        private void btnApply_Click(object sender, EventArgs e)
        {
            //Filters Recipes First By Ingredients
            FilterByIngredients();
            //The remaining recipes are then filtered by excluded cuisines
            cuisineFilter();
            //Further filters the remaining messages, allowing only the ones that fit between the min and max brackets
            nutrientFilterValidation();
            //The reamining recipes which fit the users preferred cuisine are then highlighted green
            highlightPrefferedCuisines();
        }

        //Removes recipes that belong to excluded cuisines
        private void cuisineFilter()
        {
            //Checks if each cuisine is excluded 
            foreach (DataGridViewRow row in dgvCuisines.Rows)
            {
                if (row.Cells[1].Value.ToString().Substring(0, 1) == "-")
                {
                    foreach (DataRow dr in dsRecipeFilter.Tables["RecipeCuisines"].Rows)
                    {
                        if (row.Cells[0].Value.ToString() == dr["CuisineId"].ToString())
                        {
                            foreach (DataGridViewRow row2 in dgvRecipes.Rows)
                            {
                                if (row2.Cells[0].Value.ToString() == dr["RecipeId"].ToString())
                                    dgvRecipes.Rows.Remove(row2);
                            }
                        }
                    }
                }
            }
        }

        //Omits unwanted ingredients from the recipe list
        private void FilterByIngredients()
        {
            if (dgvSearch.RowCount > 0)
            {
                dgvRecipes.Rows.Clear();
                sqlRecipeIngredients = @"Select * from RecipeIngredients";
                daRecipeIngredients = new SqlDataAdapter(sqlRecipeIngredients, connStr);
                cmdBRecipeIngredients = new SqlCommandBuilder(daRecipeIngredients);
                daRecipeIngredients.FillSchema(dsRecipeFilter, SchemaType.Source, "RecipeIngredients");
                daRecipeIngredients.Fill(dsRecipeFilter, "RecipeIngredients");

                //Checks each recipe's ingredients, if any omitted ingredients are part of the recipe, it is not added to the dgv.
                bool acceptable = true;
                foreach (DataRow dr in dsRecipeFilter.Tables["Recipes"].Rows)
                {
                    foreach (DataRow dr2 in dsRecipeFilter.Tables["RecipeIngredients"].Rows)
                    {
                        if(dr["recipeId"].ToString() == dr2["recipeId"].ToString())
                        {
                            foreach(DataGridViewRow row in dgvSearch.Rows)
                            {
                                if (row.Cells[1].Value.ToString().Contains("-") && dr2["ingredientId"].ToString() == row.Cells[0].Value.ToString())
                                {
                                    acceptable = false;
                                }
                            }                          
                        }
                    }
                    
                    if (acceptable)
                        dgvRecipes.Rows.Add(dr["RecipeId"], dr["RecipeName"]);
                    acceptable = true;
                }
            }
            else
                loadRecipes();
        }

        //Checks a value against a minimum and maximum restriction, if the value meets the requirements (if any), the method returns true
        private bool nutrientFilter(double val, String min, String max)
        {
            bool ok = true;


            if (min != "" && max != "")
            {
                if (val < Convert.ToInt32(min) || val > Convert.ToInt32(max))
                    ok = false;
            }
            else if (max != "" ) 
            {
                if (val > Convert.ToInt32(max))
                    ok = false;
            }
            else if (min != "")
            {
                if (val < Convert.ToInt32(min))
                    ok = false;
            }

            return ok;
        }

        //Checks validation for each min/max textbox and then remove unapplicable recipes
        private void nutrientFilterValidation()
        {
    
            errp.Clear();
            errorCount = 0;

            testError(txtCalMin);
            testError(txtCalMax);
            testError(txtProMin);
            testError(txtProMax);
            testError(txtCarbsMin);
            testError(txtCarbsMax);
            testError(txtFarMin);
            testError(txtFatMax);
            testError(txtSaltMin);
            testError(txtSaltMax);
            testError(txtSodiumMin);
            testError(txtSodiumMax);
            //Makes sure validation is compelte
            if (errorCount == 0)
            {
                //Checks each recipe against the minimum and maximum requirements (If any) and removes recipes which do not meet these from the DGV
                for (int i = dgvRecipes.Rows.Count - 1; i >= 0; i--)
                {
                    bool acceptable = true;
                    drRecipes = dsRecipeFilter.Tables["Recipes"].Rows.Find(dgvRecipes.Rows[i].Cells[0].Value.ToString());

                    //Calories
                    if (!nutrientFilter(Convert.ToInt32(drRecipes["Calories"].ToString()), txtCalMin.Text.ToString(), txtCalMax.Text.ToString()))
                        acceptable = false;
                    
                    //Protein
                    if (!nutrientFilter(Convert.ToDouble(drRecipes["Protein"].ToString()), txtProMin.Text.ToString(), txtProMax.Text.ToString()))
                        acceptable = false;

                    
                    //Fat
                    if (!nutrientFilter(Convert.ToDouble(drRecipes["Fat"].ToString()), txtFarMin.Text.ToString(), txtFatMax.Text.ToString()))
                        acceptable = false;

                    //Carbs
                    if (!nutrientFilter(Convert.ToDouble(drRecipes["Carbohydrates"].ToString()), txtCarbsMin.Text.ToString(), txtCarbsMax.Text.ToString()))
                        acceptable = false;
                    
                    //Salt
                    if(drRecipes["sugar"].ToString() != "")               
                        if (!nutrientFilter(Convert.ToDouble(drRecipes["sugar"].ToString()), txtSaltMin.Text.ToString(), txtSaltMax.Text.ToString()))
                            acceptable = false;
                    
                    //Sodium
                    if(drRecipes["sodium"].ToString() != "")
                        if (!nutrientFilter(Convert.ToDouble(drRecipes["sodium"].ToString()), txtSodiumMin.Text.ToString(), txtSodiumMax.Text.ToString()))                     
                            acceptable = false;
                        

                    if (!acceptable)
                            dgvRecipes.Rows.Remove(dgvRecipes.Rows[i]);
                }                
            }



            
        }

        //Validation method for textboxes that if something is entered, it is numeric
        public void testError(TextBox txt)
        {
            if (!Regex.IsMatch(txt.Text.ToString(), @"^\d+$") && txt.Text.ToString() != "")
            {
                errorCount++;
                errp.SetError(txt, "Only numeric values can be entered");
            }
        }

        //Highlights recipes in the dgv which are the same cuisine as the user's preferred cuisine
        public void highlightPrefferedCuisines()
        {
            if(userId != 0)
            {
                drUsers = dsRecipeFilter.Tables["Users"].Rows.Find(userId);
                //Checks the user has a preferred cuisine
                if (drUsers["preferredCuisine"].ToString() != "")
                {
                    foreach (DataGridViewRow row in dgvRecipes.Rows)
                    {
                        foreach (DataRow dr in dsRecipeFilter.Tables["RecipeCuisines"].Rows)
                        {
                            if (row.Cells[0].Value.ToString() == dr["RecipeId"].ToString() && drUsers["preferredCuisine"].ToString() == dr["CuisineId"].ToString())
                            {
                                row.DefaultCellStyle.BackColor = Color.Green;
                            }
                        }
                    }
                }
            }
        }

        //Adds an ingredient to the search DGV
        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvSearch.Rows.Add(dgvIngredients.CurrentRow.Cells[0].Value.ToString(), dgvIngredients.CurrentRow.Cells[1].Value.ToString());
        }

        //Removes all ingredients from the search DGV
        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvSearch.Rows.Clear();
            loadRecipes();
        }

        //Removes a single row from the search DGV
        private void btnRemove_Click(object sender, EventArgs e)
        {
            dgvSearch.Rows.Remove(dgvSearch.CurrentRow);
        }

        //Adds an ingredient as an omission to the search DGV, making it red with '-' to show it is an omission
        private void btnOmit_Click(object sender, EventArgs e)
        {
            dgvSearch.Rows.Add(dgvIngredients.CurrentRow.Cells[0].Value.ToString(), "-"+dgvIngredients.CurrentRow.Cells[1].Value.ToString());
            dgvSearch.Rows[dgvSearch.RowCount - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 204, 203);
        }

        //Loads ingredients from the database and into the ingredients dgv
        private void loadIngredients()
        {
            sqlIngredients = @"select * from Ingredients";
            daIngredients = new SqlDataAdapter(sqlIngredients, connStr);
            cmdBIngredients = new SqlCommandBuilder(daIngredients);
            daIngredients.FillSchema(dsRecipeFilter, SchemaType.Source, "Ingredients");

            daIngredients.Fill(dsRecipeFilter, "Ingredients");
            dgvIngredients.Rows.Clear();



            foreach (DataRow dr in dsRecipeFilter.Tables["Ingredients"].Rows)
            {
                dgvIngredients.Rows.Add(dr["ingredientId"], dr["ingredientName"]);
            }
        }

        //Loads recipes from the database and into the recipe dgv
        private void loadRecipes()
        {
            sqlRecipes = @"select * from Recipes";
            daRecipes = new SqlDataAdapter(sqlRecipes, connStr);
            cmdBRecipes = new SqlCommandBuilder(daRecipes);
            daRecipes.FillSchema(dsRecipeFilter, SchemaType.Source, "Recipes");
            daRecipes.Fill(dsRecipeFilter, "Recipes");
            dgvRecipes.Rows.Clear();

            foreach (DataRow dr in dsRecipeFilter.Tables["Recipes"].Rows)
            {
                dgvRecipes.Rows.Add(dr["recipeId"], dr["recipeName"]);
            }
        }

        //Loads cuisines from the database and into the cuisines dgv
        private void loadCuisines()
        {
            sqlCuisines = @"select * from Cuisine";
            daCuisines = new SqlDataAdapter(sqlCuisines, connStr);
            cmdBCuisines = new SqlCommandBuilder(daCuisines);
            daCuisines.FillSchema(dsRecipeFilter, SchemaType.Source, "Cuisine");
            daCuisines.Fill(dsRecipeFilter, "Cuisine");
            dgvCuisines.Rows.Clear();

            foreach (DataRow dr in dsRecipeFilter.Tables["Cuisine"].Rows)
            {
                dgvCuisines.Rows.Add(dr["cuisineId"], dr["cuisineName"]);
            }
        }

        //Loads user cuisines from the database and into the user cuisines dgv
        private void loadUserCuisines()
        {
            dgvUserCuisines.Rows.Clear();
            foreach (DataRow dr in dsRecipeFilter.Tables["Cuisine"].Rows)
            {
                dgvUserCuisines.Rows.Add(dr["cuisineId"], dr["cuisineName"]);
            }
        }

        //Loads recipe cuisines from the database
        private void loadRecipeCuisines()
        {
            sqlRecipeCuisines = @"select * from RecipeCuisines";
            daRecipeCuisines = new SqlDataAdapter(sqlRecipeCuisines, connStr);
            cmdBRecipeCuisines = new SqlCommandBuilder(daRecipeCuisines);
            daRecipeCuisines.FillSchema(dsRecipeFilter, SchemaType.Source, "RecipeCuisines");
            daRecipeCuisines.Fill(dsRecipeFilter, "RecipeCuisines");
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

        //When something is typed, the database returns ingredients which begin with whatever is typed and adds them to the dgv
        private void updateSearch()
        {

            connSearch = ("Select * from Ingredients where ingredientName like '" + txtSearch.Text.Trim() + "%'");
            daSearch = new SqlDataAdapter(connSearch, connStr);
            cmdBSearch = new SqlCommandBuilder(daSearch);
            dsRecipeFilter.Tables["Ingredients"].Clear();
            daSearch.FillSchema(dsRecipeFilter, SchemaType.Source, "Ingredients");
            daSearch.Fill(dsRecipeFilter, "Ingredients");
            daIngredients.Update(dsRecipeFilter, "Ingredients");
            dgvIngredients.Rows.Clear();

            foreach (DataRow dr in dsRecipeFilter.Tables["Ingredients"].Rows)
            {
                dgvIngredients.Rows.Add(dr["ingredientId"], dr["ingredientName"]);
            }
        }

        //When something is typed, the database returns recipes which begin with whatever is typed and adds them to the dgv
        private void updateRecSearch()
        {
            connSearch = ("Select * from Recipes where recipeName like '" + txtRecSearch.Text.Trim() + "%'");
            daSearch = new SqlDataAdapter(connSearch, connStr);
            cmdBSearch = new SqlCommandBuilder(daSearch);
            dsRecipeFilter.Tables["Recipes"].Clear();
            daSearch.FillSchema(dsRecipeFilter, SchemaType.Source, "Recipes");
            daSearch.Fill(dsRecipeFilter, "Recipes");
            daRecipes.Update(dsRecipeFilter, "Recipes");
            dgvRecDetails.Rows.Clear();

            foreach (DataRow dr in dsRecipeFilter.Tables["Recipes"].Rows)
            {
                dgvRecDetails.Rows.Add(dr["recipeId"], dr["recipeName"], dr["calories"], dr["protein"], dr["fat"], dr["carbohydrates"], dr["sugar"], dr["sodium"]);
            }
        }

        //Resets the text in all min/max boxes 
        private void clearMinMax()
        {
            foreach (Control c in pnlNutrients.Controls)
                if (c is TextBox)
                    c.Text = "";
        }
        
        //Validation when a user tries to log in
        private void logInValidation()
        {
            bool found = false;
            if (!Regex.IsMatch(txtLogIn.Text.ToString(), @"^\d+$") && txtLogIn.Text.ToString() != "")
            {
                errp.SetError(txtLogIn, "Only numeric values can be entered");
            }
            else if (txtLogIn.Text.ToString().Trim().Length == 0)
            {
                errp.SetError(txtLogIn, "You must enter your an ID to log in.");
            }
            else //If what is entered caused no errors and the user id is found, the global variable userid is assigned
            {
                foreach(DataRow dr in dsRecipeFilter.Tables["Users"].Rows)
                {
                    if(dr["userId"].ToString() == txtLogIn.Text.ToString().Trim())
                    {
                        userId = Convert.ToInt32(dr["userId"].ToString());
                        logIn();
                        found = true;
                    }
                    
                }
            }
            //If there are no errors in input, but the user dosen't exist, this error is thrown.
            if (!found)
                errp.SetError(txtLogIn, "User Not Found.");
        }

        //Resets the user details
        private void emptyUserDetails()
        {
            lblId.Text = "";
            txtCurWeight.Text = "";
            txtGoalWeight.Text = "";
            txtDailyGoalFat.Text = "";
            txtDailyGoalCal.Text = "";
            txtDailyGoalCarbs.Text = "";
            txtDailyGoalPro.Text = "";
            foreach (DataGridViewRow row in dgvUserCuisines.Rows)
                row.DefaultCellStyle.BackColor = Color.White;
            lblBreakFast.Text = "";
            lblLunch.Text = "";
            lblDinner.Text = "";
            lblOther.Text = "";
            lblDailyCal.Text = "";
            lblDailyCarbs.Text = "";
            lblDailyFat.Text = "";
            lblDailyPro.Text = "";
        }

        //Fills the user's details area of the user tab with applicable information
        private void logIn()
        {
            emptyUserDetails();
            btnLogOut.Enabled = true;
            btnEdit.Enabled = true;
            

            lblId.Text = ""+userId;
            drUsers = dsRecipeFilter.Tables["Users"].Rows.Find(userId);
            //Checks which fields are not empty, adding their details to the user details tab
            if (drUsers["currentWeight"].ToString() != "")
                txtCurWeight.Text = drUsers["currentWeight"].ToString();

            if (drUsers["goalWeight"].ToString() != "")
                txtGoalWeight.Text = drUsers["goalWeight"].ToString();

            if (drUsers["calorieGoal"].ToString() != "")
                txtDailyGoalCal.Text = drUsers["calorieGoal"].ToString();

            if (drUsers["proteinGoal"].ToString() != "")
                txtDailyGoalPro.Text = drUsers["proteinGoal"].ToString();

            if (drUsers["carbGoal"].ToString() != "")
                txtDailyGoalCarbs.Text = drUsers["carbGoal"].ToString();

            if (drUsers["fatGoal"].ToString() != "")
                txtDailyGoalFat.Text = drUsers["fatGoal"].ToString();

            if (drUsers["preferredCuisine"].ToString() != "")
            {
                foreach (DataGridViewRow row in dgvUserCuisines.Rows)
                    if (row.Cells[0].Value.ToString() == drUsers["preferredCuisine"].ToString())
                        row.DefaultCellStyle.BackColor = Color.Green;
            }

            loadTodaysRecipes();
        }

        //Loads user recipes from the database
        private void loadUserRecipes()
        {
            sqlUserRecipes = @"select * from UserRecipes";
            daUserRecipes = new SqlDataAdapter(sqlUserRecipes, connStr);
            cmdBUserRecipes = new SqlCommandBuilder(daUserRecipes);
            daUserRecipes.FillSchema(dsRecipeFilter, SchemaType.Source, "UserRecipes");
            daUserRecipes.Fill(dsRecipeFilter, "UserRecipes");
        }

        //Loads recipes into the recipe details dgv
        private void fillRecDetails()
        {
            dgvRecDetails.Rows.Clear();
            foreach (DataRow dr in dsRecipeFilter.Tables["Recipes"].Rows)
            {
                dgvRecDetails.Rows.Add(dr["recipeId"], dr["recipeName"], dr["calories"], dr["protein"], dr["fat"], dr["carbohydrates"], dr["sugar"], dr["sodium"]);
            }
        }

        //Displays what recipes have been eaten today by a user, as well as a cumulative view of the combined macronutrients for those recipes, compared to their goals.
        private void loadTodaysRecipes()
        {
            if (userId != 0)
            {
                double calories = 0, carbs = 0, fat = 0, protein = 0;
                String today = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
                foreach (DataRow dr in dsRecipeFilter.Tables["UserRecipes"].Rows)
                {
                    if(dr["dateLogged"].ToString().Substring(0,10) == today && Convert.ToInt32(dr["userId"].ToString()) == userId)
                    {
                        String recipeName = dsRecipeFilter.Tables["Recipes"].Rows.Find(dr["recipeId"].ToString())["recipeName"].ToString();
                        drRecipes = dsRecipeFilter.Tables["Recipes"].Rows.Find(dr["recipeId"].ToString());
                        switch (dr["mealTime"].ToString())
                        {
                            case "Breakfast": lblBreakFast.Text = drRecipes["recipeName"].ToString(); break;
                            case "Lunch": lblLunch.Text = drRecipes["recipeName"].ToString(); break;
                            case "Dinner": lblDinner.Text = drRecipes["recipeName"].ToString(); break;
                            case "Other": lblOther.Text = drRecipes["recipeName"].ToString(); break;
                            default: break;
                        }
                        calories += Convert.ToInt32(drRecipes["calories"].ToString());
                        carbs += Convert.ToDouble(drRecipes["carbohydrates"].ToString());
                        fat += Convert.ToDouble(drRecipes["fat"].ToString());
                        protein += Convert.ToDouble(drRecipes["protein"].ToString());
                    }
                }
                drUsers = dsRecipeFilter.Tables["Users"].Rows.Find(userId);
                lblDailyCal.Text = calories + "/" + drUsers["calorieGoal"].ToString();
                lblDailyPro.Text = carbs + "/" + drUsers["proteinGoal"].ToString()+" (g)";
                lblDailyCarbs.Text = fat + "/" + drUsers["carbGoal"].ToString() + " (g)";
                lblDailyFat.Text = protein + "/" + drUsers["fatGoal"].ToString() + " (g)";

            }
            else
            {
                lblBreakFast.Text = "";
                lblLunch.Text = "";
                lblDinner.Text = "";
                lblOther.Text = "";
            }
        }
    }
}
