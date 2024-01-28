using System.ComponentModel.DataAnnotations;

namespace ExpressApp.Module.Notification.Base;

public class SmtpClientOptions
{
    [Required]
    public string Host { get; set; }

    [Required]
    public string Domain { get; set; }
    
    [Required]
    public int Port { get; set; }
    
    [Required]
    public bool EnableSsl { get; set; }
    
    [Required]
    public bool UseDefaultCredentials { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}