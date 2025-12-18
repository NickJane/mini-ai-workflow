import type { AIFileRequest } from './flow.types';

export interface LocalConversation {
  id?: string | null;
  title?: string | null;
  isTop: boolean;
  lastMessageTime?: string;
  messageCount: number;
}

export interface LocalConversationsResponse {
  data?: LocalConversation[] | null;
  hasMore: boolean;
  limit: number;
}

export interface LocalMessage {
  id?: string | null;
  conversationId?: string | null;
  question?: string | null;
  answer?: string | null;
  files?: AIFileRequest[] | null;
  createdAt: string;
  totalTokens?: number | null;
}

export interface LocalMessagesResponse {
  data?: LocalMessage[] | null;
  hasMore: boolean;
  limit: number;
}

export interface UpdateConversationTitleRequest {
  title?: string | null;
}

