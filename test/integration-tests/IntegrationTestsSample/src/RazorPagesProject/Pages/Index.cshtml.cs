using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesProject.Data;
using RazorPagesProject.Services;

namespace RazorPagesProject.Pages
{
    // <snippet1>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ISampleService _sampleService;

        public IndexModel(ApplicationDbContext db, ISampleService sampleService)
        {
            _db = db;
            _sampleService = sampleService;
        }

        [BindProperty]
        public Message Message { get; set; }

        public IList<Message> Messages { get; private set; }

        [TempData]
        public string MessageAnalysisResult { get; set; }

        public string Quote { get; private set; }

        public async Task OnGetAsync()
        {
            Messages = await _db.GetMessagesAsync();

            Quote = await _sampleService.GetSampleValue();
        }
        // </snippet1>

        public async Task<IActionResult> OnPostAddMessageAsync()
        {
            if (!ModelState.IsValid)
            {
                Messages = await _db.GetMessagesAsync();

                return Page();
            }

            await _db.AddMessageAsync(Message);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllMessagesAsync()
        {
            await _db.DeleteAllMessagesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMessageAsync(int id)
        {
            await _db.DeleteMessageAsync(id);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAnalyzeMessagesAsync()
        {
            Messages = await _db.GetMessagesAsync();

            if (Messages.Count == 0)
            {
                MessageAnalysisResult = "There are no messages to analyze.";
            }
            else
            {
                var wordCount = 0;

                foreach (var message in Messages)
                {
                    wordCount += message.Text.Split(' ').Length;
                }

                var avgWordCount = Decimal.Divide(wordCount, Messages.Count);
                MessageAnalysisResult = $"The average message length is {avgWordCount:0.##} words.";
            }

            return RedirectToPage();
        }
    }
}
