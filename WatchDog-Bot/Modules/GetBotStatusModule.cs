﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchDog_Bot.Helpers;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot.Modules
{
    public class GetBotStatusModule : BotModuleBase
    {
        private Statistics Statistics { get; }
        public GetBotStatusModule(Statistics statistics)
        {
            Statistics = statistics;
        }

        [Command("dogstatus"), Alias("hojkasstatus")]
        public async Task Status()
        {
            await Status("count");
        }

        [Command("dogstatus"), Alias("hojkasstatus")]
        public async Task Status(string orderType)
        {
            var processStatus = Process.GetCurrentProcess();

            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Description = "Stav bota",
            };

            AddInlineEmbedField(embed, "Využití RAM", FormatHelper.FormatAsSize(processStatus.WorkingSet64));
            AddInlineEmbedField(embed, "Běží od", processStatus.StartTime.ToString("dd. MM. yyyy HH:mm:ss"));
            AddInlineEmbedField(embed, "Počet vláken", GetThreadStatus(processStatus));

            await ReplyAsync("", embed: embed.Build());
            await PrintCallStats(orderType == "time");
        }

        private async Task PrintCallStats(bool orderByTime)
        {
            var data = Statistics.GetOrderedData(orderByTime);

            var embedData = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Description = "Statistiky metod"
            };

            AddInlineEmbedField(embedData, "Příkaz", string.Join(Environment.NewLine, data.Select(x => x.Command)));
            AddInlineEmbedField(embedData, "Počet volání", 
                string.Join(Environment.NewLine, data.Select(x => FormatHelper.FormatWithSpaces(x.CallsCount))));
            AddInlineEmbedField(embedData, "Průměrná doba", 
                string.Join(Environment.NewLine, data.Select(o => TimeSpan.FromMilliseconds(o.AverageTime).ToString())));

            await ReplyAsync("", embed: embedData.Build());
        }

        private string GetThreadStatus(Process process)
        {
            int sleepCount = 0;
            var sleepCounter = process.Threads.GetEnumerator();
            while (sleepCounter.MoveNext())
                if ((sleepCounter.Current as ProcessThread)?.ThreadState == ThreadState.Wait)
                    sleepCount++;

            return $"{FormatHelper.FormatWithSpaces(process.Threads.Count)} ({FormatHelper.FormatWithSpaces(sleepCount)} spí)";
        }
    }
}