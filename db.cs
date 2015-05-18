using Starcounter;
using System;

namespace Chatter {

	[Database]
	public class Room {
		public string name;
	}

	[Database]
	public class Connection {
		public UInt64 socket;
		public Room room;
	}

	[Database]
	public class Statement {
		public Room room;
		public string user;
		public string msg;
		public long tick;
	}
}
