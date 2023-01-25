Create database RecipeFilter
use RecipeFilter

Create Table Ingredients(
	ingredientId		INT			NOT NULL,
	ingredientName		VARCHAR(50) NOT NULL

	CONSTRAINT pkIngredientID PRIMARY KEY (ingredientID)
)

Create Table Recipes(
	recipeId			INT				NOT NULL,
	recipeName			VARCHAR(50)		NOT NULL,
	calories			INT				NOT NULL,
	protein				FLOAT				NOT NULL,
	fat					FLOAT				NOT NULL,
	carbohydrates		FLOAT				NOT NULL,
	sugar				FLOAT,
	sodium				FLOAT

	CONSTRAINT pkRecipeID PRIMARY KEY (recipeID)
)


Create Table RecipeIngredients(
	recipeId			INT				NOT NULL,
	ingredientId		INT				NOT NULL,
)

Create Table Cuisine(
	cuisineId			INT				NOT NULL,
	cuisineName			VARCHAR(50)		NOT NULL,

	CONSTRAINT PKcuisineId PRIMARY KEY (cuisineId)
)

Create Table RecipeCuisines(
	recipeId			INT				NOT NULL,
	cuisineId			INT				NOT NULL,

	CONSTRAINT FKrecID FOREIGN KEY (recipeId) REFERENCES Recipes(recipeId),
	CONSTRAINT PKrecipeCusines PRIMARY KEY (recipeId, cuisineId)
)

Create Table Users(
	userId				INT				NOT NULL,
	currentWeight		FLOAT,
	goalWeight			FLOAT,
	calorieGoal			INT,
	proteinGoal			INT,
	carbGoal			INT,
	fatGoal				INT,
	preferredCuisine	INT,			

	CONSTRAINT PKuserID PRIMARY KEY (userId)
)

Create Table UserRecipes(
	userId				INT				NOT NULL,
	recipeId			INT				NOT NULL,
	dateLogged			DATE			NOT NULL,
	mealTime			VARCHAR(9)		NOT NULL,

	CONSTRAINT FKusID FOREIGN KEY (userId) REFERENCES Users(userId),
	CONSTRAINT FKrpID FOREIGN KEY (recipeId) REFERENCES Recipes(recipeId),
	CONSTRAINT PKuserRecipesID PRIMARY KEY (userId, recipeId, dateLogged)
)