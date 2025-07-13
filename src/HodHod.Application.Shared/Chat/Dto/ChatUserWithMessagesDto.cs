using System.Collections.Generic;

namespace HodHod.Chat.Dto;

public class ChatUserWithMessagesDto : ChatUserDto
{
    public List<ChatMessageDto> Messages { get; set; }
}

