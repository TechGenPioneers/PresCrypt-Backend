using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminContactUsService : IAdminContactUsService
    {

        private readonly ApplicationDbContext _context;
        private readonly IAdminEmailService _adminEmailService;
        public AdminContactUsService(ApplicationDbContext context, IAdminEmailService adminEmailService)
        {
            _context = context;
            _adminEmailService = adminEmailService;
        }
        public async Task<List<AdminContactUsDto>> GetAllMessages()
        {
            try
            {
                var messages = await _context.PatientContactUs
      .Select(contact => new AdminContactUsDto
      {
          InquiryId = contact.InquiryId,

          UserId = contact.UserId,

          FirstName = contact.FirstName,
          LastName = contact.LastName,
          Email = contact.Email,
          PhoneNumber = contact.PhoneNumber,
          Topic = contact.Topic,
          ReplyMessage = contact.ReplyMessage,
          IsRead = contact.IsRead,
          Description = contact.Description,
          SenderType = contact.Role
      })
      .ToListAsync();


                return messages;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AdminContactUsDto> GetMessageById(string inquiryId)
        {
            try
            {
                var message = await _context.PatientContactUs
                    .Join(
                        _context.Patient,
                        contact => contact.UserId,
                        patient => patient.PatientId,
                        (contact, patient) => new AdminContactUsDto
                        {
                            InquiryId = contact.InquiryId,


                            UserId = contact.UserId,

                            FirstName = contact.FirstName,
                            LastName = contact.LastName,
                            Email = contact.Email,
                            PhoneNumber = contact.PhoneNumber,
                            Topic = contact.Topic,
                            ReplyMessage = contact.ReplyMessage,
                            IsRead = contact.IsRead,
                            Description = contact.Description,
                            ProfileImage = patient.ProfileImage,
                            SenderType = contact.Role

                        }
                    )
                    .FirstOrDefaultAsync(m => m.InquiryId == inquiryId);


                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ReadMsg(string inquiryId)
        {
            var message = await _context.PatientContactUs
                .FirstOrDefaultAsync(m => m.InquiryId == inquiryId);

            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Message not found");
            }
        }

        public async Task<string> SendReply(AdminContactUsDto adminContactUsDto)
        {
            var message = await _context.PatientContactUs
                .FirstOrDefaultAsync(m => m.InquiryId == adminContactUsDto.InquiryId);

            

            if (message != null)
            {
                message.IsRead = true;
                message.ReplyMessage = adminContactUsDto.ReplyMessage;
                await _context.SaveChangesAsync();

                AdminContactUsDto adminContact = new AdminContactUsDto
                {
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    Email = message.Email,
                    ReplyMessage = adminContactUsDto.ReplyMessage,
                    SenderType = message.Role
                    ,
                };

                try
                {

                    await _adminEmailService.ReplyMsg(adminContact);

                }catch (Exception ex)
                {
                    throw new Exception($"Failed to send reply email: {ex.Message}");
                }

                return "Reply sent successfully.";
            }
            else
            {
                throw new Exception("Message not found.");
            }
        }

    }
}
