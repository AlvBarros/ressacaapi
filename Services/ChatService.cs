using System.Collections.Generic;
using System.Linq;
using RessacaApi.Model;

namespace RessacaApi.Services
{
    class ChatService
    {
        private ChatService() { }
        private static ChatService _instance;

        public List<Group> Groups { get; private set; }

        private static readonly object _lock = new object();
        public static ChatService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ChatService();
                        _instance.Groups = new List<Group>();
                    }
                }
            }
            return _instance;
        }

        public Group GetGroupByName(string name)
        {
            return this.Groups.FirstOrDefault(g => g.Name == name);
        }

        public Group CreateNewGroup(string userName, string userConnectionId)
        {
            Group group = Group.FromUnavailableNames(Groups.Select(g => g.Name).ToList());
            group = group.JoinGroup(userName, userConnectionId);
            Groups.Add(group);
            return group;
        }

        public Group GetGroupByUser(string connectionId) {
            return this.Groups.FirstOrDefault(g => g.Users.Any(u => u.ConnectionId.Equals(connectionId)));
        }

        public User GetUserByConnectionId(string connectionId) {
            return GetGroupByUser(connectionId).Users.FirstOrDefault(u => u.ConnectionId == connectionId);
        }

        public void RefreshGroups() {
            this.Groups = this.Groups.Where(g => g.Users.Count > 0).ToList();
        }
    }
}