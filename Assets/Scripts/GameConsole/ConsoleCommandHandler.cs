using Engine.Utilities;
using UnityEngine;

namespace GameConsole
{
	public static class ConsoleCommandHandler
	{
		private const string cmd_startsim = "/begin";
		private const string cmd_placeholder = "/placeholder";

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
				parameter = message.Substring(cmd_startsim.Length).Trim();
			}
			//Placeholder
			else if (message.StartsWith(cmd_placeholder))
			{
				command = MessageCommand.Placeholder;
				parameter = message.Substring(cmd_placeholder.Trim().Length).ToLower();
			}

			return new ChatUserCommand(command, parameter);
		}


		public static void RunCommand(ChatUserCommand command)
		{
			switch (command.Type)
			{
				case MessageCommand.BeginSimulation:
					var parameters = string.IsNullOrEmpty(command.Parameter) ? new string[0] : command.Parameter.Split(' ');
					try
					{
						int delayTimeMs = parameters.Length > 0 ? int.Parse(parameters[0]) : -1;
						int chunksPerLine = parameters.Length > 1 ? int.Parse(parameters[1]) : -1;
						NetworkHub.ClientManager.StartGeneration(delayTimeMs, chunksPerLine);
					}
					catch
					{
						Debug.Log("Error: " + command.Parameter);
						return;
					}
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
