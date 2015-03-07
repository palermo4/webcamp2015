// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

namespace GeekQuiz.Services
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using GeekQuiz.Hubs;
    using GeekQuiz.Models;
    // namespace for GlobalHost below
    using Microsoft.AspNet.SignalR;

    public class StatisticsService
    {
        private TriviaContext db;

        public StatisticsService(TriviaContext db)
        {
            this.db = db;
        }

        public async Task<StatisticsViewModel> GenerateStatistics()
        {
            var correctAnswers = await this.db.TriviaAnswers.CountAsync(a => a.TriviaOption.IsCorrect);
            var totalAnswers = await this.db.TriviaAnswers.CountAsync();
            var totalUsers = (float)await this.db.TriviaAnswers.GroupBy(a => a.UserId).CountAsync();

            var incorrectAnswers = totalAnswers - correctAnswers;

            return new StatisticsViewModel
            {
                CorrectAnswers = correctAnswers,
                IncorrectAnswers = incorrectAnswers,
                TotalAnswers = totalAnswers,
                CorrectAnswersAverage = (totalUsers > 0) ? correctAnswers / totalUsers : 0,
                IncorrectAnswersAverage = (totalUsers > 0) ? incorrectAnswers / totalUsers : 0,
                TotalAnswersAverage = (totalUsers > 0) ? totalAnswers / totalUsers : 0,
            };
        }

        // when this method is called, it updates all listening clients with new stats
        public async Task NotifyUpdates()
        {
            // get an instance of the hub for statistics communications
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<StatisticsHub>();
            // is anyone out there?
            if (hubContext != null)
            {
                // get the current stats
                var stats = await this.GenerateStatistics();
                // call every client that is connected
                // also "updateStatistics" is now attached to StatisticsHub
                hubContext.Clients.All.updateStatistics(stats);
            }
        }
    }
}