import { apiClient } from './apiClient';
import {
  ConversationSummaryDto,
  CreateConversationDto,
  MessageDto,
  SendMessageDto,
} from '../types/message';

export const messageApi = {
  getConversations: async (): Promise<ConversationSummaryDto[]> => {
    const response = await apiClient.get('/messages/conversations');
    return response.data;
  },

  getConversationById: async (conversationId: number): Promise<ConversationSummaryDto> => {
    const response = await apiClient.get(`/messages/conversations/${conversationId}`);
    return response.data;
  },

  getMessages: async (conversationId: number, page = 1, pageSize = 30): Promise<MessageDto[]> => {
    const response = await apiClient.get(`/messages/conversations/${conversationId}/items`, {
      params: { page, pageSize },
    });
    return response.data;
  },

  createConversation: async (payload: CreateConversationDto): Promise<ConversationSummaryDto> => {
    const response = await apiClient.post('/messages/conversations', payload);
    return response.data;
  },

  sendMessage: async (conversationId: number, payload: SendMessageDto): Promise<MessageDto> => {
    const response = await apiClient.post(`/messages/conversations/${conversationId}/items`, payload);
    return response.data;
  },

  markAsRead: async (conversationId: number): Promise<void> => {
    await apiClient.post(`/messages/conversations/${conversationId}/read`);
  },
};
