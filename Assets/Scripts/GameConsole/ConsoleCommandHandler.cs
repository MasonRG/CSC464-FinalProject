namespace GameConsole
{
	public static class ConsoleCommandHandler
	{
		private const string cmd_startsim = "/begin ";
		private const string cmd_placeholder = "/placeholder ";

		public static bool IsCommand(string message)
		{
			return message.StartsWith(cmd_startsim) || message.StartsWith(cmd_placeholder);
		}


		public static ChatUserCommand GetUserCommandFromMessage(string message)
		{
			MessageCommand command = MessageCommand.None;
			string parameter = "";

			//Begin Simulation
			if (message.StartsWith(cmd_startsim))
			{
				command = MessageCommand.BeginSimulation;
				parameter = message.Substring(cmd_startsim.Length);
			}
			//Placeholder
			else if (message.StartsWith(cmd_placeholder))
			{
				command = MessageCommand.Placeholder;
				parameter = message.Substring(cmd_placeholder.Length).ToLower();
			}

			return new ChatUserCommand(command, parameter);
		}


		public static void RunCommand(ChatUserCommand command, bool isFromMe)
		{
			if (!isFromMe)
				return;

			switch (command.Type)
			{
				case MessageCommand.BeginSimulation:
					break;
				case MessageCommand.Placeholder:
					break;
			}
		}
		

		public struct ChatUserCommand
		{
			readonly MessageCommand type;
			readonly string parameter;

			public MessageCommand Type { get { return type; } }
			public string Parameter { get { return parameter; } }

			public bool IsValid
			{
				get
				{
					return type != MessageCommand.None;
				}
			}

			public bool HasMessage
			{
				get
				{
					return type == MessageCommand.BeginSimulation;
				}
			}

			public ChatUserCommand(MessageCommand type, string parameter)
			{
				this.type = type;
				this.parameter = parameter;
			}
		}
	}
}
