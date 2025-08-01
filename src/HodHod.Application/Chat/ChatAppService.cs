﻿using System.Collections.Generic;
using Abp.Domain.Repositories;
using HodHod.Chat.Dto;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using HodHod.Friendships.Cache;
using HodHod.Friendships.Dto;

namespace HodHod.Chat;

[AbpAuthorize]
public class ChatAppService : HodHodAppServiceBase, IChatAppService
{
    private readonly IRepository<ChatMessage, long> _chatMessageRepository;
    private readonly IUserFriendsCache _userFriendsCache;
    private readonly IOnlineClientManager _onlineClientManager;
    private readonly IChatCommunicator _chatCommunicator;

    public ChatAppService(
        IRepository<ChatMessage, long> chatMessageRepository,
        IUserFriendsCache userFriendsCache,
        IOnlineClientManager onlineClientManager,
        IChatCommunicator chatCommunicator)
    {
        _chatMessageRepository = chatMessageRepository;
        _userFriendsCache = userFriendsCache;
        _onlineClientManager = onlineClientManager;
        _chatCommunicator = chatCommunicator;
    }

    [DisableAuditing]
    public async Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings()
    {
        var userIdentifier = AbpSession.ToUserIdentifier();
        if (userIdentifier == null)
        {
            return new GetUserChatFriendsWithSettingsOutput();
        }

        var cacheItem = _userFriendsCache.GetCacheItem(userIdentifier);
        var friends = ObjectMapper.Map<List<FriendDto>>(cacheItem.Friends);

        foreach (var friend in friends)
        {
            friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
            );
        }

        return new GetUserChatFriendsWithSettingsOutput
        {
            Friends = friends,
            ServerTime = Clock.Now
        };
    }

    [DisableAuditing]
    public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input)
    {
        var userId = AbpSession.GetUserId();
        var messages = await _chatMessageRepository.GetAll()
                .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                .OrderByDescending(m => m.CreationTime)
                .Take(input.MaxResultCount)
                .ToListAsync();

        messages.Reverse();

        return new ListResultDto<ChatMessageDto>(ObjectMapper.Map<List<ChatMessageDto>>(messages));
    }

    public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
    {
        var userId = AbpSession.GetUserId();
        var tenantId = AbpSession.TenantId;

        // receiver messages
        var messages = await _chatMessageRepository
             .GetAll()
             .Where(m =>
                    m.UserId == userId &&
                    m.TargetTenantId == input.TenantId &&
                    m.TargetUserId == input.UserId &&
                    m.ReadState == ChatMessageReadState.Unread)
             .ToListAsync();

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            message.ChangeReadState(ChatMessageReadState.Read);
        }

        // sender messages
        using (CurrentUnitOfWork.SetTenantId(input.TenantId))
        {
            var reverseMessages = await _chatMessageRepository.GetAll()
                .Where(m => m.UserId == input.UserId && m.TargetTenantId == tenantId && m.TargetUserId == userId)
                .ToListAsync();

            if (!reverseMessages.Any())
            {
                return;
            }

            foreach (var message in reverseMessages)
            {
                message.ChangeReceiverReadState(ChatMessageReadState.Read);
            }
        }

        var userIdentifier = AbpSession.ToUserIdentifier();
        var friendIdentifier = input.ToUserIdentifier();

        _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

        var onlineUserClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
        if (onlineUserClients.Any())
        {
            await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineUserClients, friendIdentifier);
        }

        var onlineFriendClients = await _onlineClientManager.GetAllByUserIdAsync(friendIdentifier);
        if (onlineFriendClients.Any())
        {
            await _chatCommunicator.SendReadStateChangeToClients(onlineFriendClients, userIdentifier);
        }
    }
}
