using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using Engine;
using Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GameConsole
{	
	public class Console : ConsoleBehavior
	{
		public GameObject messagePrefab;

		private ConsoleDisplayElements chat;
		private MessageManager messageManager;

		private bool isTyping = false;
		public bool IsTyping { get { return isTyping; } }

		public bool IsReady { get { return networkObject.NetworkReady; } }
		

		protected override void NetworkStart()
		{
			base.NetworkStart();

			chat = new ConsoleDisplayElements(transform);
			messageManager = new MessageManager(chat.content, messagePrefab);
		}


		private void Update()
		{
			float scrollVal = Input.GetAxis("Mouse ScrollWheel");
			if (scrollVal != 0)
			{
				MoveScrollBar(scrollVal);
			}
		}

		#region Local Message Writing
		public void StartWriting()
		{
			if (!IsReady) return;

			chat.ActivateInputField();

			isTyping = true;
			wasScrolled = false;
		}

		public void StopWriting()
		{
			if (!IsReady) return;

			chat.DeactivateInputField();

			isTyping = false;
			wasScrolled = false;
			CheckForScrollToBottom();
		}


		bool wasScrolled = false;
		public void MoveScrollBar(float val)
		{
			if (CheckIfScrollable())
			{
				wasScrolled = true;
				float move = Mathf.Clamp01(chat.scrollRect.verticalNormalizedPosition + (val));
				chat.scrollRect.verticalNormalizedPosition = move;
				ResetWasScrolled();
			}
		}
		
		public void OnValueChanged()
		{
			if (!isTyping) StopWriting();

			if (chat.inputField.text.EndsWith("\n"))
			{
				WriteMessage(chat.inputField);
			}
		}

		public void WriteMessage(TMP_InputField sender)
		{
			string text = sender.text.Trim();
			if (!string.IsNullOrEmpty(sender.text) && text.Length > 0)
			{
				text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
				BroadcastUserMessage(text);
				sender.text = string.Empty;
				sender.ActivateInputField();
			}
		}
		#endregion

		#region Message Broadcasting
		private void BroadcastUserMessage(string message)
		{
			BroadcastMessageInfo info = new BroadcastMessageInfo
				(
					MessageEvent.ClientMessage,
					NetworkHub.MyClient.Name,
					NetworkHub.MyClientID,
					message
				);

			BroadcastMessage_Internal(info);
		}


		public void BroadcastServerMessage(MessageEvent serverEvent, string username = "", uint userid = uint.MaxValue, string message = "")
		{
			BroadcastMessageInfo info = new BroadcastMessageInfo
				(
					serverEvent,
					username,
					userid,
					message
				);

			BroadcastMessage_Internal(info);
		}

		private void BroadcastMessage_Internal(BroadcastMessageInfo info)
		{
			networkObject.SendRpc(RPC_TRANSMIT_MESSAGE, Receivers.All, info.username, info.message, info.chatEventByte, info.userid);
		}
		#endregion


		#region RPCs (receive message)
		public override void TransmitMessage(RpcArgs args)
		{
			string username = args.GetNext<string>();
			string message = args.GetNext<string>();
			MessageEvent messageEvent = ConsoleUtils.ByteToChatEvent(args.GetNext<byte>());
			uint senderID = args.GetNext<uint>();
			bool isFromMe = NetworkHub.MyClientID == senderID;
			bool iAmLeader = NetworkHub.MyClientID == NetworkHub.LeaderID;

			if (ConsoleUtils.IsServerEvent(messageEvent))
			{
				//No command checking for server events
				PrintToChatBox(ConsoleMessageFormatter.FormatServerMessage(messageEvent, username, message, isFromMe));
			}
			else
			{
				message = ConsoleMessageFormatter.CleanMessage(message);

				//Check if this is even a valid message (cleaning might have made it empty for example)
				if (!ConsoleMessageFormatter.IsValid(message))
					return;

				//Check if this is a command message so we can run the command and print the command-specific message
				if (ConsoleCommandHandler.IsCommand(message))
				{
					var command = ConsoleCommandHandler.GetUserCommandFromMessage(message);
					if (command.IsValid)
					{
						if (command.HasMessage)
						{
							PrintToChatBox(ConsoleMessageFormatter.FormatCommandMessage(username, command.Type, command.Parameter, isFromMe, iAmLeader));
						}

						if (isFromMe && iAmLeader)
							ConsoleCommandHandler.RunCommand(command);
					}
				}
				else
				{
					PrintToChatBox(ConsoleMessageFormatter.FormatUserMessage(messageEvent, username, message, isFromMe));
				}
			}
		}
		#endregion


		#region Helpers
		private void PrintToChatBox(string formattedMessage)
		{
			messageManager.AddNewMessage(formattedMessage);
			RoutineRunner.StartFrameTimer(CheckForScrollToBottom, 1);
		}

		private bool CheckIfScrollable()
		{
			if (!IsReady) return false;
			return chat.scrollBar.IsActive() && chat.scrollBar.size < 1;
		}

		private void ResetWasScrolled()
		{
			//Reset was scrolled if we had scrolled up the chat box, but then scrolled all the way back down
			wasScrolled = !(wasScrolled && chat.scrollRect.verticalNormalizedPosition <= 0.05f);
		}

		private void CheckForScrollToBottom()
		{
			if (!IsReady) return;

			if (!wasScrolled && chat.scrollRect.verticalNormalizedPosition > 0)
				chat.scrollRect.verticalNormalizedPosition = 0;
		}
		#endregion
	}


	public struct BroadcastMessageInfo
	{
		public readonly MessageEvent chatEvent;
		public readonly byte chatEventByte;
		public readonly string username;
		public readonly uint userid;
		public readonly string message;

		public bool IsServerEvent { get { return chatEvent != MessageEvent.None && chatEvent != MessageEvent.ClientMessage; } }
		public bool IsUserMessage { get { return chatEvent == MessageEvent.ClientMessage; } }

		public BroadcastMessageInfo(MessageEvent chatEvent, string username, uint userid, string message)
		{
			this.chatEvent = chatEvent;
			chatEventByte = ConsoleUtils.ChatEventToByte(chatEvent);
			this.username = username;
			this.userid = userid;
			this.message = message;
		}

		public void PrintDebug()
		{
			Debug.Log(string.Format("Event: {0} (encoding={1})\nUsername: {2}\nUserID: {3}\nMessage: {4}",
				chatEvent, chatEventByte, username, userid, message));
		}
	}
	
}