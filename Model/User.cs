namespace RessacaApi.Model {
    class User {
        public string Name { get; private set; }
        public string ConnectionId { get; private set; }
        public string Signal {get; set; }
        public string CurrentGroup {get; private set;}

        public User(string name, string connectionId, string currentGroup) {
            this.Name = name;
            this.ConnectionId = connectionId;
            this.CurrentGroup = currentGroup;
        }
    }
}