

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using QuizApplicationMVC.Services;

namespace QuizApplicationMVC.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionsController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }




        public async Task<IActionResult> Index(int quizId)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                // If the session for the user ID is not set, redirect to login
                return RedirectToAction("Login", "Users");
            }

            // Store the passed quizId in session
            HttpContext.Session.SetInt32("QuizId", quizId);

            // Now, retrieve the quizId from session and use it to filter questions
            int? currentQuizId = HttpContext.Session.GetInt32("QuizId");

            if (currentQuizId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Fetch questions for the current quiz
            var questions = await _questionRepository.GetQuestionsByQuizIdAsync(currentQuizId.Value);

            if (questions == null || !questions.Any())
            {
                return NotFound(); 
            }

            return View(questions);
        }


        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _questionRepository.GetQuestionByIdAsync(id.Value); 
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                // Session is not set, so redirect to the login page or take appropriate action
                return RedirectToAction("Login", "Users");
            }
            if (HttpContext.Session.GetInt32("QuizId") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Questions/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionsId,QuestionsName,OptionA,OptionB,OptionC,OptionD,CorrectOption")] Questions questions)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (HttpContext.Session.GetInt32("QuizId") == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Determine the correct answer based on selected option
            string COption = questions.CorrectOption;
            switch (questions.CorrectOption)
            {
                case "A":
                    COption = questions.OptionA;
                    break;
                case "B":
                    COption = questions.OptionB;
                    break;
                case "C":
                    COption = questions.OptionC;
                    break;
                case "D":
                    COption = questions.OptionD;
                    break;
            }

            questions.CorrectOption = COption;
            questions.QuizId = (int)HttpContext.Session.GetInt32("QuizId");

            if (ModelState.IsValid)
            {
                // the question and associate it with the quiz
                await _questionRepository.AddQuestionAsync(questions, questions.QuizId);

                // Redirect to the Index method of the Questions controller with the quizId as route parameter
                return RedirectToAction("Index", "Questions", new { quizId = questions.QuizId });
            }
            else
            {
                // Log validation errors for debugging
                Console.WriteLine("Validation Errors:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            return View(questions);
        }



        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questions = await _questionRepository.GetQuestionByIdAsync(id.Value); 
            if (questions == null)
            {
                return NotFound();
            }
            return View(questions);
        }

        // POST: Questions/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuestionsName,OptionA,OptionB,OptionC,OptionD,CorrectOption")] Questions updatedQuestion)
        {
            if (id != updatedQuestion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                
                var updateResult = await _questionRepository.UpdateQuestionAsync(updatedQuestion);
                if (updateResult != null)
                {
                    // Redirect to the Index method of the Questions controller with the quizId
                    return RedirectToAction("Index", new { quizId = updateResult.QuizId });
                }
                else
                {
                    return NotFound(); 
                }
            }

            return View(updatedQuestion);
        }


        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questions = await _questionRepository.GetQuestionByIdAsync(id.Value);
            if (questions == null)
            {
                return NotFound();
            }

            return View(questions);
        }


        // POST: Questions/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {


            // Find the question by ID
            var question = await _questionRepository.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            // Get the quizId from the session
            int quizId = (int)HttpContext.Session.GetInt32("QuizId");

            // Find the quiz that contains the question
            var quiz = await _questionRepository.GetQuizByIdAsync(quizId);

            // Remove the question from the quiz's Questions list
            if (quiz != null)
            {
                quiz.Questions.Remove(question); // Remove question directly from the quiz
            }

           
            await _questionRepository.DeleteQuestionAsync(question);

            // Check if there are any questions left in the quiz
            if (quiz?.Questions.Count == 0)
            {
                // Redirect to the index of the quiz if there are no questions left
                return RedirectToAction("MyQuizes", "Quiz", new { id = quizId }); // Adjust as necessary for your Quiz controller
            }

            // Redirect to the index action with the quiz ID after successful deletion
            return RedirectToAction("Index", new { quizId });
        }


    }
}

