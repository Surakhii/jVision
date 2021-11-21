using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using jVision.Shared;
using jVision.Server.Models;
using jVision.Shared.Models;

namespace jVision.Server.Hubs
{
    public interface IBoxClient
    {
        //how u get rid these param
        Task BoxAdded(string s);
        Task UserAdded(string s);
        Task BoxUpdated(BoxDTO b);

        Task BoxUpgraded(List<BoxDTO> b);
    }
    public class BoxHub : Hub<IBoxClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SingalR Users");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
