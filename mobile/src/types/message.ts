export interface ConversationSummaryDto {
  id: number;
  participantUserId: number;
  participantName: string;
  participantRole: string;
  participantAvatarUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
  updatedAt: string;
}

export interface MessageDto {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  senderRole: string;
  content: string;
  isRead: boolean;
  createdAt: string;
  isMine: boolean;
}

export interface CreateConversationDto {
  participantUserId: number;
}

export interface SendMessageDto {
  content: string;
}
