using AIS;
using AIS.Controllers;
using AIS.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class EmailConfiguration
    {
    private readonly EmailCredentails emailCredentails;
    private readonly IServiceProvider _serviceProvider;
    private static int _emailNotConfiguredLogged;

    public EmailConfiguration(IConfiguration configuration, IServiceProvider serviceProvider = null)
        {
        if (configuration == null)
            {
            throw new ArgumentNullException(nameof(configuration));
            }

        emailCredentails = new EmailCredentails(configuration);
        _serviceProvider = serviceProvider;
        }

    private string SanitizeHeaderValue(string value, string paramName)
        {
        if (string.IsNullOrWhiteSpace(value))
            {
            return string.Empty;
            }

        if (value.Contains("\r") || value.Contains("\n"))
            {
            throw new ArgumentException("Email header values must not contain new lines.", paramName);
            }

        return value.Trim();
        }

    private MailAddress CreateSafeMailAddress(string address, string paramName)
        {
        var sanitizedAddress = SanitizeHeaderValue(address, paramName);
        return new MailAddress(sanitizedAddress);
        }

    private void AddSafeRecipient(MailAddressCollection collection, string address, string paramName)
        {
        if (collection == null)
            {
            throw new ArgumentNullException(nameof(collection));
            }

        if (!string.IsNullOrWhiteSpace(address))
            {
            collection.Add(CreateSafeMailAddress(address, paramName));
            }
        }

    private static void LogInfo(string message)
        {
        Console.WriteLine($"[EmailConfiguration] {message}");
        }

    private static void LogError(string message, Exception ex)
        {
        Console.WriteLine($"[EmailConfiguration] {message} Exception={ex}");
        }

    public bool ConfigEmail(string to = "", string cc = "", string subj = "", string body = "")
        {
        try
            {
            EmailCredentailsModel em = emailCredentails.GetEmailCredentails();
            if (!em.IsConfigured)
                {
                LogEmailNotConfigured("ConfigEmail", to, cc, subj);
                return false;
                }

            if (string.IsNullOrEmpty(to))
                throw new ArgumentException("Recipient email address is required.");

            var subject = SanitizeHeaderValue(subj, nameof(subj));
            var bodyLength = string.IsNullOrWhiteSpace(body) ? 0 : body.Length;

            LogInfo($"Preparing email send. From={em.EMAIL}; To={to}; Cc={cc}; Subject={subject}; BodyLength={bodyLength}; Host={em.Host}; Port={em.Port}");

            MailMessage mail = new MailMessage
                {
                From = CreateSafeMailAddress(em.EMAIL, nameof(em.EMAIL)),
                Subject = subject,
                Body = body
                };

            // Add recipients
            AddSafeRecipient(mail.To, to, nameof(to));
            AddSafeRecipient(mail.CC, cc, nameof(cc));

            SmtpClient SmtpServer = new SmtpClient(em.Host)
                {
                Port = em.Port,
                Credentials = new NetworkCredential(em.EMAIL, em.PASSWORD),
                EnableSsl = true // If the server requires SSL
                };

            // Send the email
            SmtpServer.Send(mail);
            LogInfo($"Email sent successfully. To={to}; Cc={cc}; Subject={subject}");
            return true;
            }
        catch (SmtpException ex)
            {
            // Log SmtpException details for debugging
            LogError("SMTP error while sending email.", ex);
            return false;
            }
        catch (Exception ex)
            {
            // Log general exceptions
            LogError("General error while sending email.", ex);
            return false;
            }
        }


    public async Task<bool> ConfigEmailAsync(string to = "", string cc = "", string subj = "", string body = "")
        {
        try
            {
            EmailCredentailsModel em = emailCredentails.GetEmailCredentails();
            if (!em.IsConfigured)
                {
                LogEmailNotConfigured("ConfigEmailAsync", to, cc, subj);
                return false;
                }

            if (string.IsNullOrEmpty(to))
                throw new ArgumentException("Recipient email address is required.");

            var subject = SanitizeHeaderValue(subj, nameof(subj));
            var bodyLength = string.IsNullOrWhiteSpace(body) ? 0 : body.Length;

            LogInfo($"Preparing async email send. From={em.EMAIL}; To={to}; Cc={cc}; Subject={subject}; BodyLength={bodyLength}; Host={em.Host}; Port={em.Port}");

            using (var mail = new MailMessage
                {
                From = CreateSafeMailAddress(em.EMAIL, nameof(em.EMAIL)),
                Subject = subject,
                Body = body
                })
                {
                AddSafeRecipient(mail.To, to, nameof(to));
                AddSafeRecipient(mail.CC, cc, nameof(cc));

                using (var smtp = new SmtpClient(em.Host)
                    {
                    Port = em.Port,
                    Credentials = new NetworkCredential(em.EMAIL, em.PASSWORD),
                    EnableSsl = true
                    })
                    {
                    await smtp.SendMailAsync(mail);
                    }
                }
            LogInfo($"Async email sent successfully. To={to}; Cc={cc}; Subject={subject}");
            return true;
            }
        catch (SmtpException ex)
            {
            LogError("SMTP error while sending async email.", ex);
            return false;
            }
        catch (Exception ex)
            {
            LogError("General error while sending async email.", ex);
            return false;
            }
        }

    private void LogEmailNotConfigured(string actionName, string to, string cc, string subject)
        {
        if (Interlocked.Exchange(ref _emailNotConfiguredLogged, 1) == 1)
            {
            return;
            }

        var db = _serviceProvider?.GetService<DBConnection>();
        if (db == null)
            {
            return;
            }

        var sessionHandler = _serviceProvider.GetService<SessionHandler>();
        var pageId = sessionHandler?.GetPageId();
        int? engId = null;
        if (sessionHandler != null && sessionHandler.TryGetActiveEngagementId(out var engagementId))
            {
            engId = engagementId;
            }

        var userPpno = sessionHandler != null && sessionHandler.TryGetUser(out var sessionUser) ? sessionUser.PPNumber : null;
        var message = $"Email send skipped because email is disabled. To={to}; Cc={cc}; Subject={subject}.";
        try
            {
            db.LogWarning("Email", nameof(EmailConfiguration), actionName, message, "Email is disabled because credentials are not configured.", pageId, engId, userPpno);
            }
        catch (Exception ex)
            {
            LogError("Failed to log email-not-configured warning.", ex);
            }
        }
    }
