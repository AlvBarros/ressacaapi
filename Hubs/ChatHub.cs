using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RessacaApi.Model;
using RessacaApi.Services;

namespace RessacaApi.Hubs
{
    static class ChatHubMethods {
        public static string GroupCreationFailed = "GroupCreationFailed";
        public static string GroupJoinFailed = "GroupJoinFailed";
        public static string JoinGroup = "JoinGroup";
        public static string UserJoined = "UserJoined";
        public static string UserLeft = "UserLeft";
        public static string ReceiveMessage = "ReceiveMessage";
        public static string SendMessage = "SendMessage";
        public static string SendOffer = "SendOffer";
        public static string GetOffer = "GetOffer";
    }
    class ChatHub : Hub
    {
        public async Task CreateGroup(string userName) {
            Console.WriteLine(string.Format("User '{0}' is trying to create a new group", userName));
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.CreateNewGroup(userName, Context.ConnectionId);
            if (group == null) {
                Console.WriteLine("Group creation failed");
                await Clients.Caller.SendAsync(ChatHubMethods.GroupCreationFailed);
            } else {
                Console.WriteLine(string.Format("Group {0} created", group.Name));
                await Groups.AddToGroupAsync(Context.ConnectionId, group.Name);
                await Clients.Caller.SendAsync(ChatHubMethods.JoinGroup, group);
            }
        }

        public async Task JoinGroup(string userName, string groupName) {
            if (groupName.Length != 4) {
                throw new Exception("Invalid group name");
            }
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.GetGroupByName(groupName);
            try {
                group = group.JoinGroup(userName, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, group.Name);
                await Clients.Caller.SendAsync(ChatHubMethods.JoinGroup, group);
                await Clients.Group(group.Name).SendAsync(ChatHubMethods.UserJoined, userName);
            }
            catch (Exception ex) {
                await Clients.Caller.SendAsync(ChatHubMethods.GroupJoinFailed, ex.Message);
            }
        }

        public async Task LeaveGroup() {
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.GetGroupByUser(Context.ConnectionId);
            if (group != null) {
                string userName = group.GetUserByConnectionId(Context.ConnectionId).Name;
                group.LeaveGroup(Context.ConnectionId);
                chatService.RefreshGroups();
                await Clients.Group(group.Name).SendAsync(ChatHubMethods.UserLeft, userName);
            }
        }

        public async Task SendMessage(string user, string message)
        {
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.GetGroupByUser(Context.ConnectionId);
            if (group != null) {
                await Clients.Group(group.Name).SendAsync(ChatHubMethods.ReceiveMessage, user, message);
            }
        }

        public async Task SendOffer(string offer) {
            Console.WriteLine("offer received!");
            ChatService chatService = ChatService.GetInstance();
            User user = chatService.GetUserByConnectionId(Context.ConnectionId);
            user.Signal = offer;
            await Clients.Group(user.CurrentGroup).SendAsync(ChatHubMethods.GetOffer, new string[2] { offer, Context.ConnectionId });
        }

        public async Task GetOffer() {
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.GetGroupByUser(Context.ConnectionId);
            List<String> signals = group.Users.Where(u => !u.ConnectionId.Equals(Context.ConnectionId)).Select(u => u.Signal).ToList<String>();
            await Clients.Caller.SendAsync("GetOffer", signals);
        }

        public override async Task OnDisconnectedAsync(Exception ex) {
            ChatService chatService = ChatService.GetInstance();
            Group group = chatService.GetGroupByUser(Context.ConnectionId);
            if (group != null) {
                string userName = group.GetUserByConnectionId(Context.ConnectionId).Name;
                group.LeaveGroup(Context.ConnectionId);
                chatService.RefreshGroups();
                await Clients.Group(group.Name).SendAsync(ChatHubMethods.UserLeft, userName);
            }
        }
    }
}