using System;
using System.Collections.Generic;
using System.Linq;

namespace RessacaApi.Model
{
    class Group
    {
        public string Name { get; private set; }
        public List<User> Users { get; private set; }

        public Group(string name)
        {
            this.Name = name;
            this.Users = new List<User>();
        }

        public User GetUserByConnectionId(string connectionId) {
            return Users.FirstOrDefault(u => u.ConnectionId == connectionId);
        }

        public Group JoinGroup(string name, string connectionId) {
            if (Users.Any(u => u.Name.Equals(name))) {
                throw new Exception("Invalid name");
            }
            else if (Users.Any(u => u.ConnectionId.Equals(connectionId))) {
                throw new Exception("Invalid connection id");
            }
            else {
                Users.Add(new User(name, connectionId, this.Name));
                return this;
            }
        }

        public void LeaveGroup(string connectionId) {
            this.Users = this.Users.Where(u => u.ConnectionId != connectionId).ToList();
        }

        public static Group FromUnavailableNames(List<string> groupNames)
        {
            string newName = Group.getRandomName();
            while (groupNames.Any(g => g.Equals(newName)))
            {
                newName = Group.getRandomName();
            }
            return new Group(newName);
        }

        private static string validCharacters = "ABCDEFGHIJK";
        private static string getRandomName()
        {
            Random rnd = new Random();
            char[] characters = new char[4];
            for (var i = 0; i < characters.Length; i++)
            {
                characters[i] = validCharacters.ElementAt<char>(rnd.Next(0, validCharacters.Length));
            }
            return new string(characters);
        }
    }
}