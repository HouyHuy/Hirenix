using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.DTOs.Message;

public class CreateConversationDto
{
    [Required]
    public ulong ParticipantUserId { get; set; }
}
