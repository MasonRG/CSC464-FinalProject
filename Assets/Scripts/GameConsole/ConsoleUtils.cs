using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameConsole
{
	public enum MessageEvent
	{
		None = 0,
		ClientJoined = 1,
		ClientLeft = 2,
		ClientMessage = 3,
		ServerMessage = 4
	}

	public enum MessageCommand
	{
		None = 0,
		BeginSimulation,
		Placeholder
	}



	public class ConsoleUtils
	{
		public static byte ChatEventToByte(MessageEvent chatEvent)
		{
			return (byte)chatEvent;
		}

		public static MessageEvent ByteToChatEvent(byte val)
		{
			return (MessageEvent)val;
		}

		public static bool IsServerEvent(MessageEvent chatEvent)
		{
			return chatEvent != MessageEvent.None && chatEvent != MessageEvent.ClientMessage;
		}
	}
}
