﻿using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;

namespace QuizApplicationMVC.Services
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationDBContext _context;

        public QuestionRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<Questions>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }
        public async Task<Questions> GetQuestionByIdAsync(int id) 
        {
            return await _context.Questions.FindAsync(id);
        }
        public async Task AddQuestionAsync(Questions question, int quizId)
        {
            question.Quiz = await _context.Quiz.FindAsync(question.QuizId);


            // Add the question to the context
            await _context.Questions.AddAsync(question);

            // Fetch the associated quiz
            var quiz = await _context.Quiz.Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            // If the quiz exists, add the question to its Questions list
            if (quiz != null)
            {
                quiz.Questions.Add(question); 
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Questions> UpdateQuestionAsync(Questions updatedQuestion)
        {
            try
            {
                // Load the existing question from the database
                var existingQuestion = await _context.Questions.FindAsync(updatedQuestion.Id);
                if (existingQuestion == null)
                {
                    return null;
                }

                // Update the fields
                existingQuestion.QuestionsName = updatedQuestion.QuestionsName;
                existingQuestion.OptionA = updatedQuestion.OptionA;
                existingQuestion.OptionB = updatedQuestion.OptionB;
                existingQuestion.OptionC = updatedQuestion.OptionC;
                existingQuestion.OptionD = updatedQuestion.OptionD;

                // Determine the correct answer based on the selected option
                string correctOption = updatedQuestion.CorrectOption;
                switch (updatedQuestion.CorrectOption)
                {
                    case "A":
                        correctOption = updatedQuestion.OptionA;
                        break;
                    case "B":
                        correctOption = updatedQuestion.OptionB;
                        break;
                    case "C":
                        correctOption = updatedQuestion.OptionC;
                        break;
                    case "D":
                        correctOption = updatedQuestion.OptionD;
                        break;
                }

                existingQuestion.CorrectOption = correctOption;

                // Update the question in the database
                _context.Update(existingQuestion);
                await _context.SaveChangesAsync();

                return existingQuestion;
            }
            catch (DbUpdateConcurrencyException)
            {
                return null; 
            }
        }
        

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return await _context.Quiz
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<bool> DeleteQuestionAsync(Questions question)
        {
            if (question == null)
            {
                return false;
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true; 
        }
    }
}
