﻿using Discord.Commands;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Grillbot.Services.MemeImages;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Nudes a další zajímavé fotky")]
    public class MemeImageModule : BotModuleBase
    {
        private MemeImagesService Service { get; }

        public MemeImageModule(MemeImagesService service)
        {
            Service = service;
        }

        [Command("nudes")]
        public async Task SendNudeAsync() => await SendAsync("nudes").ConfigureAwait(false);

        [Command("notnudes")]
        public async Task SendNotNudesAsync() => await SendAsync("notnudes").ConfigureAwait(false);

        private async Task SendAsync(string category)
        {
            var file = Service.GetRandomFile(Context.Guild, category);

            if(file == null)
            {
                await ReplyAsync("Nemám žádný obrázek.");
                return;
            }

            await Context.Channel.SendFileAsync(file);
        }
    }
}
