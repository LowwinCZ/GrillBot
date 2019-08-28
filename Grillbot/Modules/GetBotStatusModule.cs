﻿using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.Configuration;
using Grillbot.Services.Preconditions;

namespace Grillbot.Modules
{
    [Name("Stav bota")]
    [DisabledCheck(RoleGroupName = "GrillStatus")]
    [RequireRoleOrAdmin(RoleGroupName = "GrillStatus")]
    public class GetBotStatusModule : BotModuleBase
    {
        private Statistics Statistics { get; }
        private IConfiguration Config { get; }
        private AutoReplyService AutoReply { get; }

        public GetBotStatusModule(Statistics statistics, IConfiguration config, AutoReplyService autoReply)
        {
            Statistics = statistics;
            Config = config;
            AutoReply = autoReply;
        }

        [Command("grillstatus")]
        [Summary("Vypíše diagnostické informace o botovi.")]
        public async Task StatusAsync()
        {
            await StatusAsync("count");
        }

        [Command("grillstatus")]
        [Summary("Vytiskne diagnostické informace o botovi s možností vybrat si řazení statistik metod (orderType).")]
        [Remarks("Možné typy řazení jsou 'time', nebo 'count'.")]
        public async Task StatusAsync(string orderType)
        {
            var processStatus = Process.GetCurrentProcess();

            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Stav bota",
            };

            AddInlineEmbedField(embed, "Využití RAM", FormatHelper.FormatAsSize(processStatus.WorkingSet64));
            AddInlineEmbedField(embed, "Běží od", processStatus.StartTime.ToString("dd. MM. yyyy HH:mm:ss"));
            AddInlineEmbedField(embed, "Počet vláken", GetThreadStatus(processStatus));
            AddInlineEmbedField(embed, "Průměrná doba reakce", Statistics.GetAvgReactTime());
            AddInlineEmbedField(embed, "Instance", GetInstanceType());
            AddInlineEmbedField(embed, "Počet aktivních tokenů", Statistics.ChannelStats.GetActiveWebTokensCount());

            await ReplyAsync("", embed: embed.Build());
            await PrintCallStatsAsync(orderType == "time");
            await PrintAutoReplyStatsAsync();
        }

        private async Task PrintCallStatsAsync(bool orderByTime)
        {
            var data = Statistics.GetOrderedData(orderByTime);

            if (data.Count == 0)
                return;

            var embedData = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Title = "Statistiky příkazů"
            };


            AddInlineEmbedField(embedData, "Příkaz", string.Join(Environment.NewLine, data.Select(x => x.Command)));
            AddInlineEmbedField(embedData, "Počet volání", 
                string.Join(Environment.NewLine, data.Select(x => FormatHelper.FormatWithSpaces(x.CallsCount))));
            AddInlineEmbedField(embedData, "Průměrná doba", 
                string.Join(Environment.NewLine, data.Select(o => o.AverageTime + "ms")));

            await ReplyAsync(embed: embedData.Build());
        }

        private async Task PrintAutoReplyStatsAsync()
        {
            var data = AutoReply.GetStatsData();

            if (data.Count == 0)
                return;

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Title = "Statistiky automatických odpovědí"
            };

            AddInlineEmbedField(embedBuilder, "Hledaný řetězec", string.Join(Environment.NewLine, data.Select(o => o.Key)));
            AddInlineEmbedField(embedBuilder, "Počet reakcí", string.Join(Environment.NewLine, data.Select(o => o.Value)));

            await ReplyAsync(embed: embedBuilder.Build());
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

        public string GetInstanceType()
        {
            var configValue = Config["IsDevelopment"];
            if (string.IsNullOrEmpty(configValue)) return "Release";

            return Convert.ToBoolean(configValue) ? "Development" : "Release";
        }
    }
}