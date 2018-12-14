using Engine.Utilities;
using System.Linq;

namespace GameConsole
{
	public static class ConsoleMessageFormatter
	{
		private const string SELF_COLOR = "<color=#00BB00>";
		private const string OTHER_COLOR = "<color=#BBBB00>";
		private const string SERVER_COLOR = "<color=#CCCCCC>";

		private const string COLOR_O = "</color>";
		private const string ITALIC_I = "<i>";
		private const string ITALIC_O = "</i>";
		private const string BOLD_I = "<B>";
		private const string BOLD_O = "</B>";


		public static bool IsValid(string message)
		{
			//The only verification will do is making sure that the message is neither null, empty nor just whitespace
			if (string.IsNullOrEmpty(message) || message.Trim().Length == 0)
			{
				return false;
			}
			return true;
		}


		public static string CleanMessage(string message)
		{
			//Only filtering for HTML tags, so we only care if this message could contain a tag (most wont so lets check it first)
			if (!message.Contains('<') || !message.Contains('>'))
				return message;

			//we know there is a '<' and a '>' in the message
			int j = 0;
			char[] filteredMsg = new char[message.Length];
			for (int i = 0; i < message.Length; i++)
			{
				char c = message[i];
				if (c == '<')
				{
					bool closed = false;
					int k = i + 1;
					while (k < message.Length)
					{
						if (message[k] == '>')
						{
							closed = true;
							break;
						}
						k++;
					}

					if (closed)
					{
						i = k;
						continue;
					}

				}
				filteredMsg[j++] = c;
			}

			return new string(filteredMsg, 0, j);
		}

		public static string FormatServerMessage(MessageEvent serverEvent, string username, string message, bool isFromMe)
		{
			string returnVal = string.Empty;
			string temp;
			switch (serverEvent)
			{
				case MessageEvent.ClientJoined:
					temp = isFromMe ? string.Format("You ({0}) have joined the server.", username) : string.Format("{0} has joined the server.", username);
					returnVal = FormatForServer(temp);
					break;
				case MessageEvent.ClientLeft:
					temp = isFromMe ? "You have left the server." : username + " has left the server.";
					returnVal = FormatForServer(temp);
					break;
				case MessageEvent.ServerMessage:
					returnVal = FormatForServer(message);
					break;
				default:
					returnVal = FormatForServer("---- Event: " + serverEvent.ToString() + " ----");
					break;
			}
			return returnVal;
		}


		public static string FormatUserMessage(MessageEvent userEvent, string username, string message, bool isFromMe)
		{
			//Currently only userevent is PlayerMessage - so it doesnt matter
			string nameText = (isFromMe ? SELF_COLOR : OTHER_COLOR) + username + COLOR_O;
			return string.Format(BOLD_I + "{0}: " + BOLD_O + "{1}", nameText, message);
		}


		public static string FormatCommandMessage(string username, MessageCommand commandType, string commandParameter, bool isFromMe, bool iAmLeader)
		{
			if (NetworkHub.ClientIdFromName(username) != NetworkHub.LeaderID)
			{
				return "";
			}

			if (isFromMe && !iAmLeader)
			{
				return FormatForServer("Only the leader can issue commands.");
			}

			string returnVal = string.Empty;
			string temp;
			switch (commandType)
			{
				case MessageCommand.BeginSimulation:
					temp = string.Format("Simulation started by '{0}' {2} with parameters: {1}", username, commandParameter, isFromMe ? "(you)" : "");
					returnVal = FormatForServer(temp);
					break;
				case MessageCommand.Placeholder:
					temp = isFromMe ? string.Format("Placeholder command: '{0}'.", commandParameter) : string.Format("Placeholder command from {0}: {1}", username, commandParameter);
					returnVal = FormatForServer(temp);
					break;
				default:
					returnVal = FormatForServer("---- Command: " + commandType.ToString() + " ----");
					break;
			}
			return returnVal;
		}


		private static string FormatForServer(string message)
		{
			const string prefix = ITALIC_I + SERVER_COLOR;
			const string suffix = COLOR_O + ITALIC_O;
			return prefix + message + suffix;
		}
	}

}
