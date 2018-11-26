using Engine;
using System.Collections.Generic;
using UnityEngine;

namespace GameConsole
{
	public class MessageManager
	{
		readonly GameObject msgPrefab;
		readonly Transform msgParent;

		private const int max_messages = 90;
		private ObjectPool<ConsoleMessage> messagePool;

		public Queue<ConsoleMessage> ActiveMessages { get; private set; }

		public MessageManager(Transform msgParent, GameObject msgPrefab)
		{
			this.msgParent = msgParent;
			this.msgPrefab = msgPrefab;

			messagePool = new ObjectPool<ConsoleMessage>(CreateMessage, HideMessage, GetMessage);
			ActiveMessages = new Queue<ConsoleMessage>();
		}


		private ConsoleMessage CreateMessage()
		{
			var cm = (Object.Instantiate(msgPrefab, msgParent)).GetComponent<ConsoleMessage>();
			cm.MessageParent = msgParent;
			return cm;
		}

		private void HideMessage(ConsoleMessage message)
		{
			if (message.IsActive) message.Disable();
		}

		private void GetMessage(ConsoleMessage message)
		{
			if (!message.IsActive) message.Enable();
		}


		public ConsoleMessage AddNewMessage(string message)
		{
			//If we have too many messages, we need to start returning them
			//return a third of our messages at once
			if (ActiveMessages.Count >= max_messages)
			{
				for (int i = 0; i < max_messages / 3; i++)
				{
					var msg = ActiveMessages.Dequeue();
					messagePool.ReturnObject(msg);
				}
			}

			ConsoleMessage newMessage = messagePool.GetNextObject();
			ActiveMessages.Enqueue(newMessage);

			newMessage.Text.SetText(message);

			//Debug.Log(string.Format("Pool Avail: {0}  ;  Pool Active: {1}\nIn Queue: {2}", messagePool.AvailableItems, messagePool.ActiveItems, activeMessages.Count));

			return newMessage;
		}
	}
}
