
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using QuizApplicationMVC.Services;

namespace QuizApplicationMVC.Controllers
{
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ApplicationDBContext _context;

        public QuizController(IQuizRepository quizRepository, ApplicationDBContext context)
        {
            _quizRepository = quizRepository;
            _context = context;
        }

        // GET: Quiz

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated"); // Clear TempData
            }

            // Get quizzes with questions
            var quizzesWithQuestions = await _quizRepository.GetQuizzesWithQuestionsAsync();

            // Get quizzes without questions to remove
            var quizzesToRemove = quizzesWithQuestions
                .Where(q => !q.Questions.Any()).ToList();

            // Remove empty quizzes
            await _quizRepository.RemoveQuizzesAsync(quizzesToRemove);
            ViewData["Id"] = HttpContext.Session.GetInt32("Id");
            return View(quizzesWithQuestions);
        }




        public async Task<IActionResult> MyQuizes()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = HttpContext.Session.GetInt32("Id") ?? 0;

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated"); // Clear TempData
            }

            // Retrieve the list of quizzes associated with the userId
            var quizzes = await _quizRepository.GetQuizzesByUserIdAsync(userId);

            // Log the UserId for debugging purposes
            foreach (var quiz in quizzes)
            {
                Console.WriteLine(quiz.UserId);
            }

            if (quizzes != null)
            {
                return View(quizzes);
            }
            else
            {
                //return View();
                return Problem("No quizzes found for the user or the entity set is null.");
            }
        }

        // GET: Quiz/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the quiz details using the repository
            var quiz = await _quizRepository.GetQuizDetailsAsync(id);

            Console.WriteLine("quiz.User");
            Console.WriteLine(quiz?.User);

            if (quiz == null)
            {
                return NotFound();
            }

            
            Console.WriteLine($"Number of Questions: {quiz.Questions.Count}");

            return View(quiz); // Pass the quiz object to the view
        }


     

        [HttpPost, ActionName("Evaluate")]
        [ValidateAntiForgeryToken]
        public IActionResult Evaluate(List<QuizQuestion> quizQuestions)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                return RedirectToAction("Index", "Home"); // Redirect to the home page


            }

            if (quizQuestions == null || !quizQuestions.Any())
            {
                return NotFound();
            }

            // Perform the evaluation here
            if (ModelState.IsValid)
            {
                int quizId = -1;
                // ModelState is valid, proceed with evaluation
                int correctAnswers = 0;
                foreach (var newQuestion in quizQuestions)
                {

                    var originalQuestion = _context.QuizQuestion.FirstOrDefault(q => q.Id == newQuestion.Id);
                    Console.WriteLine("newQuestion.Id");
                    Console.WriteLine(newQuestion.Id);

                    if (originalQuestion != null)
                    {
                        originalQuestion.SelectedOption = newQuestion.SelectedOption;
                        _context.SaveChanges();
                        Console.WriteLine(originalQuestion.SelectedOption);

                        if (newQuestion.SelectedOption == originalQuestion.CorrectOption)
                        {
                            correctAnswers++;
                        }
                        quizId = originalQuestion.QuizId;
                    }
                    else
                    {
                        Console.WriteLine("not found asjb");
                    }
                }

                TempData["QuizEvaluated"] = true; // Set a flag

                TempData["CorrectAnswersCount"] = correctAnswers;

                int userId = (int)HttpContext.Session.GetInt32("Id");

                Console.WriteLine("userId");
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                Console.WriteLine(quizId);
                var quiz = _context.Quiz.FirstOrDefault(q => q.Id == quizId);
                //Console.WriteLine(quiz.Title);

                var quizUserHistory = new QuizUserHistory
                {
                    UserId = userId,
                    QuizId = quizId,
                    Score = correctAnswers,
                    DateTaken = DateTime.Now,
                    User = user,
                    Quiz = quiz
                };

                Console.WriteLine("Calculated");
                // Add the new QuizUserHistory entry to the database
                _context.QuizUserHistory.Add(quizUserHistory);
                _context.SaveChanges();


                return View("EvaluationResult", quizQuestions);
            }
            else
            {
                // Log validation errors
                foreach (var key in ModelState.Keys)
                {
                    var modelStateEntry = ModelState[key];
                    foreach (var error in modelStateEntry.Errors)
                    {
                        var errorMessage = error.ErrorMessage;
                        Console.WriteLine($"Validation Error for {key}: {errorMessage}");
                    }
                }
                return View("Take", quizQuestions);
            }
        }

        public IActionResult EvaluationResult()
        {
            return View();
        }



        public async Task<IActionResult> Take(int? id)
        {
            if (id == null || _context.Quiz == null)
            {
                return NotFound();
            }

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated"); // Clear TempData
                return RedirectToAction("Index", "Home"); // Redirect to the home page
            }

            var questions = await _context.Questions
                .Where(q => q.QuizId == id)
                .ToListAsync();
            Console.WriteLine("Questions count: " + questions.Count);

            if (questions == null || !questions.Any())
            {
                return NotFound();
            }

            // Create a list of QuizQuestion objects for the questions in the quiz
            var quizQuestions = questions.Select(q => new QuizQuestion
            {
                QuestionName = q.QuestionsName,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                SelectedOption = "",
                CorrectOption = q.CorrectOption,
                QuizId = (int)id
            }).ToList();
            Console.WriteLine(quizQuestions);
            foreach (var question in quizQuestions)
            {
                _context.Add(question);
            }

            await _context.SaveChangesAsync();
            // Retrieve the quiz to get the duration
            var quiz = await _quizRepository.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound(); // Handle the case where the quiz is not found
            }

            // Pass the duration to the view
            ViewBag.Duration = quiz.Duration;

           
            return View(quizQuestions);
        }



        // GET: Quiz/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quiz/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Duration")] Quiz quiz)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = (int)HttpContext.Session.GetInt32("Id");

            if (ModelState.IsValid)
            {
                var createdQuiz = await _quizRepository.CreateQuizAsync(quiz, userId);
                if (createdQuiz != null)
                {
                    // Store the created quiz's ID in the session
                    HttpContext.Session.SetInt32("QuizId", createdQuiz.Id);
                    return RedirectToAction("Create", "Questions", null);
                }
                else
                {
                    Console.WriteLine("User not found for the provided ID.");
                }
            }
            else
            {
                Console.WriteLine("Validation Errors:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            Console.WriteLine("Returning to Quiz creation view due to validation errors.");
            return View(quiz);
        }

        // GET: Quiz/Edit/5


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizRepository.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quiz/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Duration")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing quiz from the repository
                    var existingQuiz = await _quizRepository.GetQuizByIdAsync(id);
                    if (existingQuiz == null)
                    {
                        return NotFound();
                    }

                    // Update the properties of the existing quiz
                    existingQuiz.Title = quiz.Title;
                    existingQuiz.Description = quiz.Description;
                    existingQuiz.Duration = quiz.Duration; // Make sure to update the Duration as well

                    // Update the quiz in the repository
                    await _quizRepository.UpdateQuizAsync(existingQuiz);

                    return RedirectToAction(nameof(MyQuizes)); // Redirect after successful update
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_quizRepository.QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Rethrow exception if quiz exists
                    }
                }
            }

            return View(quiz);
        }





        // GET: Quiz/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizRepository.GetQuizByIdAsync(id.Value);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }



        // POST: Quiz/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_quizRepository.QuizExists(id))
            {
                return NotFound();
            }

            await _quizRepository.DeleteQuizWithHistoryAsync(id);
            return RedirectToAction(nameof(MyQuizes));
        }

    }
}