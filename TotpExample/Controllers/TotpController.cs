using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TotpExample.Helpers;
using TotpExample.Models;

namespace TotpExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TotpController : ControllerBase
    {
        private readonly MemDbContext _context;

        public TotpController(MemDbContext context)
        {
            context.Database.EnsureCreated();

            _context = context;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll(TotpEnrollModel request)
        {
            // Fetch current user details from database (for demo purposes this will just select the first user)
            User user = await _context.Users.FirstOrDefaultAsync();

            // Generate the base32 secret key
            string secretB32 = TOTPHelper.GenerateSecret();
            
            // Create the URI so that it can be scanned via QR code
            var uri = TOTPHelper.BuildAuthUri(Constants.APP_NAME, user.Email, secretB32);
            
            // Convert QR code to base64 so that we can display it in the browser
            var qrCodeB64 = TOTPHelper.GenerateQrPng(uri);

            // Encrypt the TOTP secret with the user's password.
            var secret = CryptoHelper.Encrypt(secretB32, request.Password);
            user.Secret = secret;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    secret = secretB32,
                    qrCode = qrCodeB64
                }
            });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] TotpValidateModel request)
        {
            User user = await _context.Users.FirstOrDefaultAsync();

            // Decrypt the secret using the user's password
            string secret = CryptoHelper.Decrypt(user.Secret, request.Password);

            // Validate that the secret is correct
            bool valid = TOTPHelper.Validate(request.Code, secret);

            return valid ? Ok() : BadRequest();
        }

        #region Models
        public class TotpEnrollModel
        {
            [Required]
            public string Password { get; set; }
        }
        public class TotpValidateModel
        {
            [Required]
            public string Password { get; set; }
            [Required]
            public string Code { get; set; }
        }
        #endregion
    }
}
