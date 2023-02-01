﻿using System;
using System.Net.NetworkInformation;

namespace Genshin_Impact_MP_Installer.Scripts
{
	internal abstract class Internet
	{
		private static bool Ping(string host)
		{
			try
			{
				Ping ping = new Ping();
				byte[] buffer = new byte[32];
				const int timeout = 5000;
				PingOptions options = new PingOptions();
				PingReply reply = ping.Send(host, timeout, buffer, options);
				return reply != null && reply.Status == IPStatus.Success;
			}
			catch (Exception ex)
			{
				Log.ErrorAndExit(new Exception($"Sorry. I cannot connect to the {host} server. Please check your network or antivirus program and try again.\n\n• Error:\n{ex.Message}"), false, false);

				return false;
			}
		}

		public static bool CheckConnection()
		{
			return Ping("sefinek.net") && Ping("api.sefinek.net") && Ping("cdn.sefinek.net") && Ping("discordapp.com");
		}
	}
}