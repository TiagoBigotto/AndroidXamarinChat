﻿using System;
using Chat;
using System.Collections.Generic;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class ChatClient : ServerEventsClient
	{
		public ChatClient (string[] channels)
			: base ("http://chat.servicestack.net/", channels)
		{
			this.RegisterNamedReceiver<ChatReceiver> ("cmd");
			this.RegisterNamedReceiver<TvReciever> ("tv");
		}
			
		public void SendMessage(PostChatToChannel request)
		{
			this.ServiceClient.Post (request);
		}

		public void ChangeChannel(string channel, ChatCmdReciever cmdReceiver)
		{
			var currentChannels = new List<string> (this.Channels);
			if (cmdReceiver.FullHistory.ContainsKey (channel) && currentChannels.Contains (channel)) {
				cmdReceiver.ChangeChannel (channel);
			} else {
				currentChannels.Add (channel);
				this.Channels = currentChannels.ToArray ();
				if (Channels != null && Channels.Length > 0)
					this.EventStreamUri = this.EventStreamUri
						.AddQueryParam("channel", string.Join(",", Channels));
				this.Restart ();
				this.UpdateChatHistory (this.Channels, cmdReceiver).ContinueWith (t => {
					cmdReceiver.ChangeChannel (channel);
				});
			}
		}

		public void StartChat(ChatCmdReciever cmdReceiver)
		{
			this.Start ();
			this.UpdateChatHistory (this.Channels, cmdReceiver).ContinueWith (t => {
				cmdReceiver.ChangeChannel (cmdReceiver.CurrentChannel);
			});
		}
	}
}