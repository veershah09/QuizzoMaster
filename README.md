# QuizzoMaster
QuizzoMaster

# Overview
The QuizzoMaster is a web-based quiz platform that allows users to create, manage, and participate in quizzes. The application supports CRUD (Create, Read, Update, Delete) operations for quizzes, questions, and user data. Each quiz includes a timer, and users can view their quiz history, scores, and detailed profiles. The application is built using ASP.NET MVC and uses Entity Framework for data management.

# Features
- User Management: Users can sign up, log in, and manage their profiles.
- Quiz Creation: Users can create quizzes with a title, description, and set a timer for quiz duration.
- Question Management: For each quiz, users can add, update, and delete questions with multiple choices.
- Quiz Participation: Users can take quizzes within a specified time limit.
- Quiz History: Users can view their quiz history, including scores and the date the quiz was taken.
- CRUD Operations: Full CRUD operations are supported for users, quizzes, and questions.

# Technologies Used
- ASP.NET Core MVC: Framework for building the application.
- Entity Framework Core: ORM for database interactions.
- SQL Server: Database to store user, quiz, and question data.
- JavaScript/HTML/CSS: Frontend design and functionality.

# How It Works
- User Registration & Login:
Users register an account by providing their name, email, and password.
After login, users are redirected to their dashboard where they can create and manage quizzes.

- Creating a Quiz:
Users create a quiz by providing a title, description, and duration.
Users can add multiple-choice questions to the quiz, specifying four options and the correct answer.

- Taking a Quiz:
When a user takes a quiz, a timer starts based on the quiz duration.
Users select answers for each question. Once the quiz is complete (or time runs out), the quiz is auto-submitted.

- Viewing Quiz History:
Users can view their quiz history, including scores and the date the quiz was taken.

- Quiz Timer:
A timer is implemented to enforce time limits for each quiz.
If time runs out before the quiz is completed, it will be auto-submitted.


# Setup Instructions
1. Clone the repository to your local machine.
   
2. Open the project in Visual Studio.

3. Configure the database connection in the appsettings.json file:
   - "ConnectionStrings": { "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuizDB;Trusted_Connection=True;MultipleActiveResultSets=true" }

4. Run the Entity Framework migrations to create the database:
   - Update-Database

5. Start the application:
   - dotnet run
