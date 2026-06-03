using Hirenix.Application.DTOs.Message;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(List<ConversationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConversationSummaryDto>>> GetConversations()
    {
        var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _messageService.GetConversationsAsync(userId);
        return Ok(items);
    }

    [HttpGet("conversations/{id:long}")]
    [ProducesResponseType(typeof(ConversationSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationSummaryDto>> GetConversationById(long id)
    {
        if (id < 0)
        {
            return BadRequest(new { message = "Invalid conversation id" });
        }

        var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversation = await _messageService.GetConversationAsync(userId, (ulong)id);
        if (conversation == null)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        return Ok(conversation);
    }

    [HttpGet("conversations/{id:long}/items")]
    [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        try
        {
            if (id < 0)
            {
                return BadRequest(new { message = "Invalid conversation id" });
            }

            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var messages = await _messageService.GetMessagesAsync(userId, (ulong)id, page, pageSize);
            return Ok(messages);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized messages read for conversation {ConversationId}: {Message}", id, ex.Message);
            return Forbid();
        }
    }

    [HttpPost("conversations")]
    [ProducesResponseType(typeof(ConversationSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConversationSummaryDto>> CreateConversation([FromBody] CreateConversationDto dto)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var conversation = await _messageService.CreateConversationAsync(userId, dto);
            return CreatedAtAction(nameof(GetConversationById), new { id = conversation.Id }, conversation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("conversations/{id:long}/items")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MessageDto>> SendMessage(long id, [FromBody] SendMessageDto dto)
    {
        try
        {
            if (id < 0)
            {
                return BadRequest(new { message = "Invalid conversation id" });
            }

            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var message = await _messageService.SendMessageAsync(userId, (ulong)id, dto);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized send message for conversation {ConversationId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("conversations/{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        try
        {
            if (id < 0)
            {
                return BadRequest(new { message = "Invalid conversation id" });
            }

            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updated = await _messageService.MarkAsReadAsync(userId, (ulong)id);
            return Ok(new { message = "Conversation marked as read", updated });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized mark-as-read for conversation {ConversationId}: {Message}", id, ex.Message);
            return Forbid();
        }
    }
}
