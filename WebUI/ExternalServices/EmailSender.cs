using Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.ExternalServices
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			Console.WriteLine("EMAİLİNİZ GELDİİİİİİİ", htmlMessage);
			return Task.CompletedTask;
		}
	}
}
